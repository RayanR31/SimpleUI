# FlowUI

A lightweight, designer-friendly UI navigation framework for Unity.

FlowUI simplifies page navigation, overlays and transitions without spaghetti code or complex setups.

![Demo](Documentation/demo.gif)

---

## Features

- Stack-based navigation
- Overlay system
- Back navigation handling
- Transition support
- Navigation lock during transitions
- Mobile-friendly workflow
- Lightweight runtime architecture

---

## Supported UI System

Supported:
- Unity uGUI (Canvas system)

Not supported:
- UI Toolkit
- IMGUI

---

## Quick Start

1. Add `UIManager` to your scene
2. Create UI pages
3. Add the `Page` component
4. Assign unique IDs
5. Add `NavButton` components
6. Press Play

---

## Navigation API

```csharp
UIManager.Instance.OpenTo("settings");
UIManager.Instance.OpenOverlay("pause");
UIManager.Instance.Back();
```

---

## Main Concepts

### Main Stack
Handles main pages:
- Home
- Settings
- Gameplay

### Overlay Stack
Handles popups:
- Pause menu
- Confirm dialogs
- Notifications

---

## Transitions

FlowUI supports optional transitions using UnityEvents.

Compatible with:
- Animator
- CanvasGroup
- Timeline
- Custom scripts

---

## Requirements

- Unity Canvas UI system
- Canvas
- EventSystem

---

## Full Documentation

See full documentation here:

[FlowUI Documentation](Documentation/FlowUI_Documentation.pdf)

---

## Author

Rayan R.
