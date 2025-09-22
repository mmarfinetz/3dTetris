using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TetrisJenga.EditorIntegration
{
	/// <summary>
	/// Lightweight HTTP bridge that lets an external MCP server ask the Unity Editor
	/// to perform safe actions (create scripts, run menu items, query project info).
	/// This runs only in the Editor.
	/// </summary>
	[InitializeOnLoad]
	public static class McpUnityBridge
	{
		private const string k_Url = "http://127.0.0.1:5174/";
		private static HttpListener _listener;
		private static Thread _listenerThread;
		private static volatile bool _running;
		private static List<LogEntry> _capturedLogs = new List<LogEntry>();
		private static readonly object _logLock = new object();

		static McpUnityBridge()
		{
			// Start automatically when the Editor domain loads
			Start();

			// Register log callback
			Application.logMessageReceived += OnLogMessageReceived;

			EditorApplication.quitting += Stop;
			AppDomain.CurrentDomain.DomainUnload += (sender, e) => Stop();
		}

		private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			lock (_logLock)
			{
				// Keep only last 100 logs
				if (_capturedLogs.Count >= 100)
				{
					_capturedLogs.RemoveAt(0);
				}

				_capturedLogs.Add(new LogEntry
				{
					logType = type.ToString(),
					message = condition,
					stackTrace = stackTrace ?? ""
				});
			}
		}

		[MenuItem("Tools/MCP/Restart Bridge")]
		public static void Restart()
		{
			Stop();
			Start();
		}

		[MenuItem("Tools/MCP/Clear Captured Logs")]
		public static void ClearLogs()
		{
			lock (_logLock)
			{
				_capturedLogs.Clear();
			}
			UnityEngine.Debug.Log("[MCP] Cleared captured console logs");
		}

		[MenuItem("Tools/MCP/Test Console Capture")]
		public static void TestConsoleCapture()
		{
			UnityEngine.Debug.Log("[MCP] Test Log message");
			UnityEngine.Debug.LogWarning("[MCP] Test Warning message");
			UnityEngine.Debug.LogError("[MCP] Test Error message");
		}

		private static void Start()
		{
			if (_running) return;
			try
			{
				_listener = new HttpListener();
				_listener.Prefixes.Add(k_Url);
				_listener.Start();
				_running = true;

				_listenerThread = new Thread(ListenLoop) { IsBackground = true, Name = "MCP Unity Bridge" };
				_listenerThread.Start();

				UnityEngine.Debug.Log($"[MCP] Unity Bridge started at {k_Url}");
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError($"[MCP] Failed to start bridge: {ex.Message}");
			}
		}

		private static void Stop()
		{
			try
			{
				_running = false;
				_listener?.Stop();
				_listener?.Close();
				_listener = null;
				if (_listenerThread != null && _listenerThread.IsAlive)
				{
					_listenerThread.Join(100);
				}

				// Unregister log callback
				Application.logMessageReceived -= OnLogMessageReceived;

				UnityEngine.Debug.Log("[MCP] Unity Bridge stopped.");
			}
			catch { }
		}

		private static void ListenLoop()
		{
			while (_running)
			{
				HttpListenerContext ctx = null;
				try
				{
					ctx = _listener.GetContext();
				}
				catch
				{
					if (!_running) break;
					continue;
				}

				HandleContext(ctx);
			}
		}

		private static void HandleContext(HttpListenerContext ctx)
		{
			try
			{
				var req = ctx.Request;
				var res = ctx.Response;
				res.ContentType = "application/json";

				if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/health")
				{
					WriteJson(res, new Health { status = "ok" });
					return;
				}

				if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/project-info")
				{
					WriteJson(res, GetProjectInfo());
					return;
				}

				if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/run-menu")
				{
					string body = ReadBody(req);
					string menuPath = GetJsonValue(body, "menuPath");
					bool ok = !string.IsNullOrEmpty(menuPath) && EditorApplication.ExecuteMenuItem(menuPath);
					WriteJson(res, new Result { ok = ok, message = ok ? "executed" : "failed" });
					return;
				}

				if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/create-script")
				{
					string body = ReadBody(req);
					string path = GetJsonValue(body, "path");
					string content = GetJsonValue(body, "content");
					if (string.IsNullOrEmpty(path)) path = "Assets/NewScript.cs";

					string dir = Path.GetDirectoryName(path);
					if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
					File.WriteAllText(path, string.IsNullOrEmpty(content) ? DefaultCSharpTemplate() : content, Encoding.UTF8);
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
					WriteJson(res, new Result { ok = true, message = path });
					return;
				}

				if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/select-asset")
				{
					string body = ReadBody(req);
					string assetPath = GetJsonValue(body, "path");
					UnityEngine.Object obj = string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadMainAssetAtPath(assetPath);
					Selection.activeObject = obj;
					WriteJson(res, new Result { ok = obj != null, message = obj ? obj.name : "not-found" });
					return;
				}

				if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/console-logs")
				{
					WriteJson(res, GetConsoleLogs());
					return;
				}

				res.StatusCode = 404;
				WriteText(res, "{\"error\":\"not found\"}");
			}
			catch (Exception ex)
			{
				try
				{
					ctx.Response.StatusCode = 500;
					WriteText(ctx.Response, "{\"error\":\"" + Escape(ex.Message) + "\"}");
				}
				catch { }
			}
			finally
			{
				try { ctx.Response.OutputStream.Close(); } catch { }
			}
		}

		private static ProjectInfo GetProjectInfo()
		{
			var active = EditorSceneManager.GetActiveScene().path;
			var scenes = new List<string>();
			foreach (var s in EditorBuildSettings.scenes) scenes.Add(s.path);
			return new ProjectInfo
			{
				unityVersion = Application.unityVersion,
				activeScene = string.IsNullOrEmpty(active) ? "" : active,
				scenesInBuild = scenes.ToArray()
			};
		}

		private static ConsoleLogs GetConsoleLogs()
		{
			List<LogEntry> logsCopy;

			lock (_logLock)
			{
				// Return a copy of the captured logs
				logsCopy = new List<LogEntry>(_capturedLogs);
			}

			return new ConsoleLogs { logs = logsCopy.ToArray(), count = logsCopy.Count };
		}

		private static string ReadBody(HttpListenerRequest req)
		{
			using (var reader = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		// Minimal JSON string value extractor (avoids bringing JSON libs into Editor scripts)
		private static string GetJsonValue(string json, string key)
		{
			if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return null;
			string pattern = "\"" + key + "\"";
			int i = json.IndexOf(pattern, StringComparison.Ordinal);
			if (i < 0) return null;
			i = json.IndexOf(':', i);
			if (i < 0) return null;
			i++;
			while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
			if (i >= json.Length) return null;
			if (json[i] == '"')
			{
				i++;
				var sb = new StringBuilder();
				for (; i < json.Length; i++)
				{
					char c = json[i];
					if (c == '\\')
					{
						i++;
						if (i < json.Length) sb.Append(json[i]);
						continue;
					}
					if (c == '"') break;
					sb.Append(c);
				}
				return sb.ToString();
			}
			else
			{
				int j = i;
				while (j < json.Length && ",}\n\r \t".IndexOf(json[j]) == -1) j++;
				return json.Substring(i, j - i);
			}
		}

		private static void WriteJson(HttpListenerResponse res, object obj)
		{
			string json = JsonUtility.ToJson(obj);
			WriteText(res, json);
		}

		private static void WriteText(HttpListenerResponse res, string text)
		{
			byte[] buf = Encoding.UTF8.GetBytes(text);
			res.ContentLength64 = buf.Length;
			res.OutputStream.Write(buf, 0, buf.Length);
		}

		private static string Escape(string s) => s?.Replace("\"", "\\\"") ?? string.Empty;

		private static string DefaultCSharpTemplate()
		{
			return "using UnityEngine;\n\npublic class NewScript : MonoBehaviour\n{\n\tprivate void Start() { }\n\tprivate void Update() { }\n}\n";
		}

		[Serializable]
		private class Health { public string status; }

		[Serializable]
		private class Result { public bool ok; public string message; }

		[Serializable]
		private class ProjectInfo
		{
			public string unityVersion;
			public string activeScene;
			public string[] scenesInBuild;
		}

		[Serializable]
		private class ConsoleLogs
		{
			public LogEntry[] logs;
			public int count;
		}

		[Serializable]
		private class LogEntry
		{
			public string logType;
			public string message;
			public string stackTrace;
		}
	}
}


