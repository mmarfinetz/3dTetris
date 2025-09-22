# 🚀 Deploying 3D Tetris Tower Online

This guide will help you build and deploy your Unity 3D Tetris game to the web so others can play it online.

## 📋 Prerequisites

- Unity 2021.3 or later (with WebGL build support installed)
- A web hosting service account (Netlify, Vercel, Firebase, or GitHub Pages)
- Git (optional, for version control)

## 🔧 Step 1: Configure Unity Build Settings

### In Unity Editor:

1. **Open Build Settings**
   - Go to `File → Build Settings`
   - Select `WebGL` platform
   - Click `Switch Platform` if not already selected

2. **Player Settings**
   - Click `Player Settings` button
   - Configure the following:

   **Resolution and Presentation:**
   - Default Canvas Width: 1280
   - Default Canvas Height: 720
   - WebGL Template: Custom3DTetris (if available) or Default

   **Publishing Settings:**
   - Compression Format: Gzip (smaller files) or Disabled (faster loading)
   - Enable "Decompression Fallback" if using compression
   - Data caching: Enable for faster subsequent loads

   **Other Settings:**
   - Color Space: Gamma (better WebGL performance)
   - Lightmap Encoding: Low Quality (smaller build)
   - Target WebGL 2.0

3. **Quality Settings**
   - Go to `Edit → Project Settings → Quality`
   - For WebGL, set default quality to "Medium" or "Low"

## 🏗️ Step 2: Build the Game

1. In Build Settings, click `Build`
2. Create a new folder called `WebGLBuild` in your project root
3. Select this folder and click `Choose`
4. Wait for the build to complete (this may take 10-30 minutes)

### Build Output Structure:
```
WebGLBuild/
├── index.html
├── Build/
│   ├── WebGLBuild.data
│   ├── WebGLBuild.framework.js
│   ├── WebGLBuild.loader.js
│   └── WebGLBuild.wasm
└── TemplateData/
    ├── favicon.ico
    └── style.css
```

## 🌐 Step 3: Deploy to Web Hosting

### Option A: Netlify (Recommended - Free & Easy)

1. **Create Account**: Sign up at [netlify.com](https://netlify.com)

2. **Deploy via Drag & Drop**:
   - Open Netlify dashboard
   - Drag your `WebGLBuild` folder to the deployment area
   - Your game will be live in seconds!

3. **Custom Domain** (optional):
   - Go to Site Settings → Domain Management
   - Add your custom domain

4. **Using Git** (optional):
   ```bash
   git init
   git add WebGLBuild/*
   git commit -m "Initial WebGL build"
   git remote add origin YOUR_GITHUB_REPO
   git push -u origin main
   ```
   - Connect GitHub repo to Netlify for automatic deployments

### Option B: Vercel

1. **Install Vercel CLI**:
   ```bash
   npm i -g vercel
   ```

2. **Deploy**:
   ```bash
   cd WebGLBuild
   vercel
   ```

3. Follow prompts to complete deployment

### Option C: Firebase Hosting

1. **Install Firebase CLI**:
   ```bash
   npm install -g firebase-tools
   ```

2. **Initialize Firebase**:
   ```bash
   firebase login
   firebase init hosting
   ```

3. **Deploy**:
   ```bash
   firebase deploy
   ```

### Option D: GitHub Pages (Free with GitHub)

1. **Create GitHub Repository**

2. **Push Build Files**:
   ```bash
   git init
   git checkout -b gh-pages
   git add WebGLBuild/*
   git commit -m "Deploy WebGL build"
   git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
   git push -u origin gh-pages
   ```

3. **Enable GitHub Pages**:
   - Go to Settings → Pages
   - Source: Deploy from branch
   - Branch: gh-pages
   - Folder: / (root)

4. **Access your game**:
   `https://YOUR_USERNAME.github.io/YOUR_REPO/`

## ⚡ Step 4: Optimization Tips

### Reduce Build Size:
1. **Texture Compression**: Use compressed texture formats
2. **Strip Engine Code**: Enable code stripping in Player Settings
3. **Audio**: Use compressed audio formats (MP3/OGG)
4. **Models**: Optimize mesh complexity

### Improve Performance:
1. **Object Pooling**: Already implemented for pieces
2. **LOD**: Use Level of Detail for complex models
3. **Batching**: Enable static/dynamic batching
4. **Mobile**: Reduce quality settings for mobile devices

## 🐛 Troubleshooting

### Common Issues:

**1. CORS Errors**
- Solution: Headers are configured in deployment files (netlify.toml, vercel.json)

**2. Large Build Size**
- Enable Gzip/Brotli compression
- Use texture atlases
- Remove unused assets

**3. Poor Performance**
- Lower quality settings
- Reduce shadow resolution
- Disable unnecessary post-processing

**4. Mobile Issues**
- Test on actual devices
- Implement touch controls
- Optimize for lower specs

## 📱 Mobile Support

The game includes basic mobile detection. To add touch controls:

1. Implement touch input in `InputManager.cs`
2. Add on-screen buttons in UI
3. Test on various devices

## 📊 Analytics (Optional)

Add analytics to track players:

```javascript
// Add to index.html
<script async src="https://www.googletagmanager.com/gtag/js?id=YOUR_ID"></script>
```

## 🎮 Share Your Game!

Once deployed, share your game URL:

- Social media with #Unity3D #WebGL #IndieGame
- Reddit: r/Unity3D, r/WebGames
- itch.io for wider reach
- Game development forums

## 📝 Update Process

To update your deployed game:

1. Make changes in Unity
2. Build again to `WebGLBuild` folder
3. Deploy:
   - **Netlify**: Drag & drop new build
   - **Vercel**: Run `vercel --prod`
   - **Firebase**: Run `firebase deploy`
   - **GitHub Pages**: Commit and push changes

## 🔗 Useful Resources

- [Unity WebGL Documentation](https://docs.unity3d.com/Manual/webgl.html)
- [WebGL Build Optimization](https://docs.unity3d.com/Manual/webgl-building.html)
- [Netlify Docs](https://docs.netlify.com)
- [Vercel Docs](https://vercel.com/docs)
- [Firebase Hosting](https://firebase.google.com/docs/hosting)

## ✅ Checklist Before Deployment

- [ ] Test locally using Unity's Build and Run
- [ ] Check console for errors
- [ ] Verify all controls work
- [ ] Test on different browsers (Chrome, Firefox, Safari)
- [ ] Check mobile responsiveness
- [ ] Optimize build size (target < 50MB)
- [ ] Add loading screen/progress bar
- [ ] Include game instructions
- [ ] Set appropriate meta tags for SEO

## 🎉 Congratulations!

Your 3D Tetris Tower game is now online! Players worldwide can enjoy your creation.

**Quick Start Commands:**
```bash
# Netlify (after setup)
netlify deploy --prod --dir=WebGLBuild

# Vercel
vercel --prod

# Firebase
firebase deploy

# GitHub Pages
git add . && git commit -m "Update" && git push
```

Happy deploying! 🚀