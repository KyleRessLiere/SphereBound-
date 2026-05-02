# Feature Index

This file is the source of truth for feature names and tracking.

Rules:
- Add every new feature here when its requirements spec is created.
- Assign feature IDs in ascending order starting at `00`.
- Feature IDs are assigned only when the feature is actually created.
- Use a single feature folder at `/specDev/features/[feature-id]-[feature-slug]/`.
- Put that feature's `requirements.md`, `design.md`, and `tasks.md` inside its own folder.
- Update status here as features move through the workflow.

## Status Key

- `[idea]` feature named but requirements not started
- `[requirements]` requirements drafted, waiting for user validation in chat
- `[design]` design drafted, waiting for user validation in chat
- `[tasks]` task list drafted, ready for execution
- `[in-progress]` implementation task currently being executed
- `[done]` implementation complete

## Features

- [00] [done] Core Combat Loop (`specDev/features/00-core-combat-loop/`)
- [01] [done] Core Scenario Runner (`specDev/features/01-core-scenario-runner/`)
- [02] [done] In-Repo Core Runner (`specDev/features/02-in-repo-core-runner/`)
- [03] [done] Runner Console Log Output (`specDev/features/03-runner-console-log-output/`)
- [04] [done] Unity Listener Bridge (`specDev/features/04-unity-listener-bridge/`)
- [05] [done] Unity Debug Action Controls (`specDev/features/05-unity-debug-action-controls/`)
- [06] [done] Unity Combat Debug Surface (`specDev/features/06-unity-combat-debug-surface/`)
- [07] [done] Unity Combat Debug File Output (`specDev/features/07-unity-combat-debug-file-output/`)
