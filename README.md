# Orbit - Desktop Automation Engine

A personal desktop automation platform for Windows built with C# and .NET 9.

Orbit combines global hotkeys, a command palette, workflow automation, application lifecycle management, execution history, real-time monitoring, and extensible action plugins into a single developer-focused automation engine.

---

## Solution Structure

```
Orbit.sln

src/
  Orbit.Engine/          # Worker Service - hotkeys, command/workflow execution, plugin loading
  Orbit.Core/            # Contracts and shared domain (interfaces, models) - no infra deps
  Orbit.Actions/         # Built-in action implementations
  Orbit.Infrastructure/  # Windows platform integrations (hotkeys, window detection, keyboard)
  Orbit.Persistence/     # SQLite + EF Core repositories
  Orbit.Dashboard/       # Blazor Server monitoring dashboard

tests/
  Orbit.UnitTests/
  Orbit.IntegrationTests/
```

---

## Core Concepts

- **Commands** – named triggers (via hotkey, palette, dashboard, or API). Reference a workflow.
- **Workflows** – YAML files composed of sequential action steps.
- **Actions** – discrete operations (launch process, type text, wait, open browser, etc.)
- **Context** – active application, current folder, window title — available to actions.

---

## Workflow Definition (YAML)

Stored in `/workflows`:

```yaml
name: start-dev-environment
steps:
  - action: launch-process
    executable: rancher-desktop.exe
  - action: wait-for-process
    process: rancher-desktop
  - action: launch-process
    executable: code.exe
  - action: open-gitbash
    folder: currentFolder
```

## Command Definition (YAML)

Stored in `/commands`:

```yaml
name: start-rancher
workflow: rancher-start
```

## Hotkey Configuration

```yaml
hotkeys:
  ctrl+alt+t:
    command: insert-build-id
  ctrl+alt+g:
    command: open-gitbash-here
  ctrl+shift+r:
    command: start-rancher
```

---

## Built-in Actions

| Action                | Description                              |
|-----------------------|------------------------------------------|
| `launch-process`      | Launch a process by executable           |
| `kill-process`        | Kill a process by name                   |
| `wait`                | Sleep for N ms/seconds                   |
| `wait-for-process`    | Wait until a process is running          |
| `type-text`           | Inject text via keyboard simulation      |
| `timestamp-text`      | Type a timestamp-based string            |
| `open-gitbash`        | Open Git Bash at a folder                |
| `open-browser`        | Open a URL in the default browser        |
| `run-powershell`      | Execute a PowerShell script              |

---

## MVP Milestone

- [ ] Global hotkey registration and command dispatch
- [ ] YAML workflow + command loading
- [ ] Sequential workflow execution with logging
- [ ] Core actions: launch-process, kill-process, wait, timestamp-text, open-gitbash
- [ ] SQLite persistence (WorkflowRuns, StepRuns, Commands, Workflows)
- [ ] Blazor Dashboard: home, workflow history, command library

## Future Enhancements

- Command Palette (Avalonia UI, `Ctrl+Shift+Space`)
- Plugin system (load custom actions from assemblies)
- Workflow scheduling (daily, hourly, cron)
- REST API (`POST /api/commands/{name}`)
- Remote agents (execute on multiple machines)
- AI integration (natural language → command)
- Visual workflow designer

---

## Tech Stack

- .NET 9 / C#
- Blazor Server (Dashboard)
- SQLite + EF Core (Persistence)
- YamlDotNet (Workflow definitions)
- Microsoft.Extensions.Logging (Structured logging)
