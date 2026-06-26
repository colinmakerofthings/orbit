# Orbit - Desktop Automation Engine

A CLI-first desktop automation platform for Windows built with C# and .NET 9.

Orbit is a workflow automation engine — not a macro recorder. It feels like `git`, `docker`, or `kubectl` for desktop automation.

```bash
orbit run insert-build-id
orbit start rancher
orbit stop rancher
orbit start dev
orbit workflows
orbit history
orbit status
orbit logs start-dev
```

---

## Solution Structure

```
Orbit.slnx

src/
  Orbit.Core/            # Contracts and shared domain (interfaces, models) — no infra deps
  Orbit.Engine/          # Workflow execution engine, command dispatcher
  Orbit.Cli/             # CLI entry point (orbit <command>)
  Orbit.Actions/         # Built-in action implementations
  Orbit.Infrastructure/  # Windows platform (hotkeys, window detection, keyboard input)
  Orbit.Persistence/     # SQLite + EF Core repositories

tests/
  Orbit.UnitTests/
  Orbit.IntegrationTests/
```

---

## How It Works

```
Hotkey / CLI
      ↓
  Workflow
      ↓
  Actions
```

- **Workflows** – YAML files composed of sequential action steps (`/workflows`)
- **Actions** – small reusable units (launch-process, kill-process, wait, type-text, etc.)
- **Context** – active application, current folder, window title — resolved at runtime by actions
- **Hotkeys** – invoke workflows directly via global key bindings

---

## Workflow Format (YAML)

Stored in `/workflows`:

```yaml
name: start-dev
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

## Hotkey Configuration

```yaml
hotkeys:
  ctrl+alt+t:
    workflow: insert-build-id
  ctrl+alt+g:
    workflow: open-gitbash-here
  ctrl+shift+r:
    workflow: rancher-start
```

---

## Built-in Actions

| Action            | Description                              |
|-------------------|------------------------------------------|
| `launch-process`  | Launch a process by executable           |
| `kill-process`    | Kill a process by name                   |
| `wait`            | Sleep for N ms/seconds                   |
| `wait-for-process`| Wait until a process is running          |
| `type-text`       | Inject text via keyboard simulation      |
| `timestamp-text`  | Type a timestamp-based string            |
| `open-gitbash`    | Open Git Bash at a folder                |
| `open-browser`    | Open a URL in the default browser        |
| `run-powershell`  | Execute a PowerShell script              |

---

## Future

- Plugin system (`/plugins` — load actions from external assemblies)
- Scheduling (`orbit schedule add dailychecks 08:00`)
- REST API (`POST /api/workflows/start-dev`)
- AI natural language → workflow mapping
- Command palette (`Ctrl+Space` → `Orbit >`)

---

## Tech Stack

- .NET 9 / C#
- Spectre.Console (CLI output)
- SQLite + EF Core (Persistence)
- YamlDotNet (Workflow definitions)
- Microsoft.Extensions.Logging (Structured logging)
- Microsoft.Extensions.DependencyInjection
