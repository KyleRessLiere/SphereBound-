# Spherebound - Agent Instructions

This project uses STRICT spec-driven development.

Codex must follow this workflow for ALL features.

---

## REQUIRED WORKFLOW

Before any implementation:

1. Check /specDev/decisions
2. Check /specDev/index.md
3. Check /specDev/features/[feature-id]-[feature-slug]/

---

## PHASE RULES

### 1. Requirements Phase
- If no requirements file exists:
  -> Create /specDev/features/[feature-id]-[feature-slug]/requirements.md
- Assign the next feature ID from /specDev/index.md, starting at `00` and incrementing by 1 for each new feature
- Register the feature in /specDev/index.md before moving forward
- Feature IDs are assigned only when a feature is actually created in the requirements phase
- Create a dedicated git branch for each new feature before implementation work begins
- Use a branch name that matches the feature, such as `feature/[feature-id]-[feature-slug]`
- DO NOT write code

### 2. Design Phase
- Only after user validation in chat
- Create /specDev/features/[feature-id]-[feature-slug]/design.md
- DO NOT write code

### 3. Task Phase
- Only after user validation in chat
- Create /specDev/features/[feature-id]-[feature-slug]/tasks.md
- Tasks must be:
  - small
  - sequential
  - testable

### 4. Execution Phase
- Only implement ONE task at a time
- Must reference a task from /specDev/features/[feature-id]-[feature-slug]/tasks.md
- Must update task status after completion
- Keep implementation work for that feature on its dedicated branch

---

## GIT WORKFLOW

- Every new feature must be developed on its own git branch
- Do not implement a new feature directly on `main`
- When a feature is complete, stop and ask the user for confirmation before committing and pushing
- Only after user confirmation:
  - commit the feature work
  - push the feature branch
- Do not auto-push completed feature work without the user's explicit confirmation in chat

## HARD RULES (DO NOT BREAK)

- Do NOT implement features without spec files
- Do NOT skip phases
- Do NOT contradict /specDev/decisions
- Do NOT invent new architecture without documenting it
- If conflict exists -> STOP and report it
- Do NOT treat agent output as approval; approval must come from the user in chat

---

## EVENT SYSTEM RULES

- All gameplay flows through events
- Effects react to events (never direct mutation)
- Use request -> resolve -> result pattern

---
## ARCHITECTURE RULE

Spherebound uses a pure C# gameplay core with Unity as presentation/input.

Do not put gameplay rules directly in MonoBehaviours unless explicitly approved.

Core game logic must not depend on GameObject, Transform, Animator, or scene state.

Unity should listen to core events and visualize outcomes.

## DONE CRITERIA

A task is complete when:
- It satisfies acceptance criteria
- It does not break existing specs
- It updates /specDev/features/[feature-id]-[feature-slug]/tasks.md
- The user has had a chance to confirm whether the completed feature should be committed and pushed

---

## FILE STRUCTURE

/specDev
  /decisions
  /features
    /[feature-id]-[feature-slug]
      requirements.md
      design.md
      tasks.md
  index.md

`/specDev/index.md` is the source of truth for feature IDs, names, and active feature tracking.

---

## BEHAVIOR

Codex should:
- Ask for clarification when needed
- Prefer minimal changes
- Follow existing patterns
- Keep system consistent

Codex should NOT:
- Jump to implementation
- Over-engineer
- Ignore spec files

---

If unsure at any step:
-> Ask before proceeding
