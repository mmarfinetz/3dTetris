# AI Visualization Control Room

## Overview

This project now includes a comprehensive suite of **real-time visualization dashboards** for watching AI evolution and training. These tools transform the learning process into an observable, interactive experience.

## 🎯 Core Features

### 1. **GA Evolution Dashboard** (`ga_visualization.html`)

A **live genetic algorithm dashboard** that shows evolution in real-time.

**Features:**
- 🔴 **Live generation ticker** - Shows current generation with key metrics
- 📈 **Real-time fitness evolution chart** - Tracks best/avg fitness over time
- 🧬 **Dynamic genome population grid** - Visual cards for each genome with:
  - Fitness bars showing relative performance
  - Elite/Average/New badges
  - Mini weight visualizations
  - Active playing indicators with glow effects
- 🎨 **Diversity tracking** - Visual diversity bar with exploration mode indicators
- 📊 **Population callouts** - Smart alerts for rapid improvement or plateaus
- ⚡ **Speed controls** - 1×, 5×, or MAX speed training

**Data Sources:**
- Reads from `localStorage` key: `tetris_ai_weights_v2_population`
- Can connect to API endpoints: `/api/genomes/elite`, `/api/strategies/distribution`

**How to Use:**
1. Open `ga_visualization.html` in your browser
2. If you have training data in localStorage, it will visualize automatically
3. Use speed controls to adjust refresh rate
4. Start/pause/reset training with control buttons

---

### 2. **Genome Lab** (`local-trainer.html`)

A **local training environment** with real-time training curves and genome management.

**Features:**
- 🧪 **Interactive training controls** - Start, pause, stop with speed selection
- 📉 **Live training curves** - Large chart showing fitness evolution with:
  - Best fitness line (green)
  - Average fitness line (blue)
  - Diversity overlay (yellow)
  - Annotations for milestone achievements
- ✨ **Sparkline metrics** - Mini charts on each metric card
- 🔬 **Active genome display** - Visual cards for each genome showing:
  - Weight distribution bars
  - Training status (elite/training indicators)
  - Real-time fitness updates
- 📊 **Diversity analysis** - Separate chart tracking population diversity
- 💾 **Export functionality** - Save best genomes to JSON files

**Controls:**
- **Start Training** - Begin evolution process
- **Pause** - Temporarily halt training
- **Stop** - End training session
- **Speed: 1×/5×/10×/MAX** - Control training speed
- **Step 1 Generation** - Manually advance one generation
- **Export Best** - Save top genome to file

**Use Cases:**
- Local experimentation with GA parameters
- Quick testing of different weight configurations
- Offline training without server dependency
- Educational demonstrations of evolutionary algorithms

---

### 3. **Admin Dashboard** (`admin-dashboard.html`)

A **comprehensive system overview** with convergence analysis and elite tracking.

**Features:**
- 📊 **System overview metrics**:
  - Total genomes in system
  - Active trainers count
  - Total games played
  - Current generation
- 📈 **Convergence graph** - Large chart showing fitness convergence over time
- 🏆 **Elite genomes list** - Top 5 performers with rankings
- 🎯 **Strategy distribution comparison**:
  - Population average weights
  - Top 5 average weights
  - Side-by-side bar visualizations
- 🕐 **Recent activity timeline** - Scrollable log of system events
- ⚡ **Performance metrics panel**:
  - Best/average fitness
  - Diversity score
  - Mutation rate
  - Generation time
- 💚 **Server health indicator** - Live heartbeat with status

**API Integration:**
The dashboard can connect to these endpoints:
- `GET /api/health` - Server status
- `GET /api/genomes/elite` - Top performing genomes
- `GET /api/strategies/distribution` - Weight distribution data
- `GET /api/activity/recent` - Recent system events

**Quick Actions:**
- ▶️ Start training
- ⏸️ Pause training
- 💾 Export all data
- 🔄 Reset system

---

### 4. **AI Decision Visualizer** (`ai-visualizer.html`)

Watch the AI **"think" in real-time** with visual decision-making displays.

**Features:**
- 🎮 **Game state display** - Visual game board with current piece
- 🔥 **Position heatmap overlay** - Color-coded candidate positions:
  - 🟢 Green = Best moves (high scores)
  - 🟡 Yellow = Good moves (medium scores)
  - 🔴 Red = Bad moves (low scores)
- 💭 **Thought bubbles** - Real-time AI reasoning display
- 🔍 **Position evaluations panel** - Ranked list showing:
  - Column and rotation for each candidate
  - Score breakdown (height, lines, holes, bumpiness)
  - Visual indicators for best/worst positions
- 🎯 **Two-ply lookahead visualization** - Shows next move planning
- 📊 **Decision metrics**:
  - Positions evaluated count
  - Best score found
  - Decision time in milliseconds
- ⚙️ **Current strategy weights display** - Live weight bars

**Toggle Controls:**
- **Show Heatmap** - Enable/disable position heatmap
- **Show Lookahead** - Display two-step planning
- **Show Thoughts** - Enable AI thought bubbles
- **Slow Motion** - Reduce decision speed for observation

**Legend:**
- 🟢 Best Move
- 🟡 Good Move
- 🔴 Bad Move

---

### 5. **Generation Time Travel** (`generation-compare.html`)

Compare different generations **side-by-side** to see evolution progress.

**Features:**
- 🎚️ **Dual generation sliders** - Select any two generations to compare
- 📊 **Side-by-side panels** showing:
  - Generation number and badge
  - Performance metrics (best/avg fitness, diversity)
  - Weight distribution visualizations
  - Visual genome representations
- 🎨 **Weight comparison charts** - Canvas-based bar charts
- 📈 **Comparison analysis**:
  - Fitness improvement percentage
  - Generations apart
  - Strategy shift (Euclidean distance)
  - Learning rate (improvement per generation)
- ⏱️ **Timeline scrubber** - Smooth scrolling through generations
- ▶️ **Auto-play mode** - Automatically advance through evolution

**Use Cases:**
- See how much the AI improved from Gen 0 to Gen 100
- Identify when major strategy shifts occurred
- Compare early flailing vs. late-game optimization
- Create time-lapse videos of evolution
- Educational demonstrations of learning progress

**Controls:**
- **Generation A/B Sliders** - Select generations to compare
- **Compare Generations** - Load selected generations
- **Auto-Play Evolution** - Animate through generations
- **Timeline Scrubber** - Scrub through entire evolution history

---

## 🎨 Design Philosophy

### Color Semantics (Consistent Across All Dashboards)

- 🟢 **Green** → Performance / Fitness / Success
- 🔵 **Blue/Purple** → Strategy / Weights / Learning
- 🟡 **Yellow** → Uncertainty / Exploration / Diversity
- 🔴 **Red** → Death / Elimination / Poor Performance
- 🔷 **Cyan** (#00ffcc) → UI accents / Live indicators
- 🟣 **Purple gradients** → Elite status / Premium features

### UI/UX Enhancements

All dashboards feature:
- ✨ **Smooth animations** - Fade ins, slides, pulses
- 🎭 **Visual feedback** - Hover effects, active states
- 📱 **Responsive design** - Works on various screen sizes
- 🌙 **Dark theme** - Easy on the eyes for long sessions
- 🔔 **Toast notifications** - Non-intrusive alerts
- 💫 **Glassmorphism effects** - Modern frosted glass styling

---

## 🚀 Getting Started

### Quick Start

1. **Open the main page:**
   ```
   open index.html
   ```

2. **Navigate to any dashboard** from the AI Visualization Control Room section

3. **No build required** - All dashboards are pure HTML/CSS/JavaScript

### With Training Data

If you have training data in localStorage:

```javascript
// Example data structure
const gaState = {
  generation: 42,
  population: [
    {
      id: 'genome-1',
      fitness: 8500,
      weights: { height: -0.51, lines: 0.76, holes: -0.36, bumpiness: -0.18 },
      evaluated: true,
      isElite: true
    },
    // ... more genomes
  ],
  history: [
    { generation: 0, bestFitness: 1200, avgFitness: 800, diversity: 0.75 },
    { generation: 1, bestFitness: 1350, avgFitness: 900, diversity: 0.70 },
    // ... more history
  ],
  config: {
    populationSize: 20,
    mutationRate: 0.15,
    explorationBalance: 0.5
  }
};

// Save to localStorage
localStorage.setItem('tetris_ai_weights_v2_population', JSON.stringify(gaState));
```

Then refresh any dashboard to see live data!

### Demo Mode

All dashboards include **automatic demo data generation** if no real data exists. This lets you:
- Explore features without setup
- See how visualizations work
- Test UI responsiveness
- Share with others easily

---

## 📊 Data Flow

```
┌─────────────────────┐
│  Training System    │
│  (Your Game/AI)     │
└──────────┬──────────┘
           │
           ├─── localStorage (tetris_ai_weights_v2_population)
           │
           └─── API Endpoints (/api/*)
                    │
        ┌───────────┴───────────┐
        ▼                       ▼
┌──────────────┐        ┌──────────────┐
│ GA Evolution │        │ Admin Dash   │
│ Dashboard    │        │              │
└──────────────┘        └──────────────┘
        │                       │
        └───────────┬───────────┘
                    ▼
            ┌──────────────┐
            │ Visualizers  │
            │ (All Dashboards) │
            └──────────────┘
```

---

## 🎯 Integration Guide

### For Existing Tetris AI Projects

**1. Add localStorage Save:**

```javascript
function saveGAState(generation, population, history, config) {
  const state = {
    generation,
    population,
    history,
    config,
    timestamp: Date.now()
  };

  localStorage.setItem('tetris_ai_weights_v2_population', JSON.stringify(state));
}

// Call after each generation
saveGAState(currentGen, currentPopulation, fitnessHistory, gaConfig);
```

**2. Add API Endpoints (Optional):**

```javascript
// Express.js example
app.get('/api/health', (req, res) => {
  res.json({ status: 'ok', timestamp: Date.now() });
});

app.get('/api/genomes/elite', (req, res) => {
  const elite = population.slice(0, 5);
  res.json(elite);
});

app.get('/api/strategies/distribution', (req, res) => {
  const avgWeights = calculateAverageWeights(population);
  res.json({ avgWeights });
});
```

**3. Link from Your Game:**

```html
<a href="ga_visualization.html" target="_blank">
  Watch Evolution Live
</a>
```

---

## 🔧 Customization

### Modify Refresh Rates

```javascript
// In each dashboard's JavaScript
this.updateInterval = 500; // milliseconds (default: 500ms)
```

### Change Color Scheme

All colors are defined in CSS. Search for these color codes:
- `#00ffcc` - Cyan accent
- `#10b981` - Green (performance)
- `#6366f1` - Blue (strategy)
- `#fbbf24` - Yellow (exploration)
- `#ef4444` - Red (danger)

### Adjust Population Size

```javascript
// In ga_visualization.html and local-trainer.html
const POPULATION_SIZE = 20; // Change this
```

---

## 📚 Advanced Features

### Annotations on Charts

The training curves support **automatic annotations** for milestone events:
- New best fitness found (gold marker)
- Generation milestones (every 10/25/50 generations)
- Strategy shift detection
- Convergence warnings

### Export Formats

**Best Genome Export:**
```json
{
  "genome": {
    "id": "genome-0",
    "fitness": 8500,
    "weights": {
      "height": -0.51,
      "lines": 0.76,
      "holes": -0.36,
      "bumpiness": -0.18
    }
  },
  "generation": 42,
  "timestamp": 1699564800000
}
```

**Full System Export:**
```json
{
  "generation": 42,
  "population": [...],
  "history": [...],
  "config": {...},
  "exportedAt": "2024-11-13T10:30:00Z"
}
```

---

## 🎓 Educational Use

These visualizations are perfect for:
- **Teaching genetic algorithms** - Visual, intuitive understanding
- **Research presentations** - Live demos of evolution
- **Student projects** - Engaging way to show ML concepts
- **Streaming** - Entertaining content for AI/gaming streams
- **Documentation** - Videos/screenshots for papers

---

## 🐛 Troubleshooting

**Q: No data showing in dashboards?**
- Check browser console for errors
- Verify localStorage has the correct key
- Try demo mode first (should work out of the box)

**Q: Charts not rendering?**
- Ensure browser supports Canvas API
- Check canvas dimensions in CSS
- Try refreshing the page

**Q: Slow performance?**
- Reduce update interval (increase milliseconds)
- Decrease population size
- Use MAX speed sparingly

**Q: API endpoints not working?**
- Dashboards fall back to localStorage/demo mode
- Check CORS settings on your server
- Verify endpoint URLs match

---

## 🚀 Future Enhancements

Potential additions:
- WebSocket support for even lower latency
- 3D weight space visualizations
- Neural network layer visualizations
- Replay functionality (save/load sessions)
- A/B testing between different GA configs
- Tournament bracket visualizations
- Fitness landscape 3D plots
- Weight correlation heatmaps

---

## 📄 File Structure

```
3dTetris/
├── index.html                    # Main landing page with links
├── ga_visualization.html         # GA Evolution Dashboard
├── local-trainer.html            # Genome Lab / Local Trainer
├── admin-dashboard.html          # Admin Dashboard
├── ai-visualizer.html            # AI Decision Visualizer
├── generation-compare.html       # Generation Time Travel
└── VISUALIZATION_README.md       # This file
```

---

## 🎉 Summary

You now have a **complete "watch the brains evolve" control room** with:

✅ **Live GA Dashboard** - Real-time evolution tracking
✅ **Local Trainer** - Hands-on training with curves
✅ **Admin Dashboard** - System-wide overview
✅ **Decision Visualizer** - AI thinking made visible
✅ **Time Travel** - Compare generations side-by-side

All features are:
- 🎨 Beautifully designed with consistent color semantics
- ⚡ Real-time updates with smooth animations
- 📱 Responsive and mobile-friendly
- 🧪 Demo mode for instant exploration
- 🔌 Easy to integrate with existing systems
- 📚 Well-documented and customizable

**Now go watch your AI learn in style!** 🚀🧬✨
