# Spherebound - Agent Instructions

This project uses STRICT spec-driven development.

Codex must follow this workflow for ALL features.

---

## REQUIRED WORKFLOW

Before any implementation:

1. Check /specDev/decisions
2. Check /specDev/requirements
3. Check /specDev/design
4. Check /specDev/tasks

---

## PHASE RULES

### 1. Requirements Phase
- If no requirements file exists:
  -> Create /specDev/requirements/[feature].md
- Register the feature in /specDev/index.md before moving forward
- DO NOT write code

### 2. Design Phase
- Only after user validation in chat
- Create /specDev/design/[feature].md
- DO NOT write code

### 3. Task Phase
- Only after user validation in chat
- Create /specDev/tasks/[feature].md
- Tasks must be:
  - small
  - sequential
  - testable

### 4. Execution Phase
- Only implement ONE task at a time
- Must reference a task from /specDev/tasks
- Must update task status after completion

---

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
- It updates /specDev/tasks

---

## FILE STRUCTURE

/specDev
  /requirements
  /design
  /tasks
  /decisions
  index.md

`/specDev/index.md` is the source of truth for feature names and active feature tracking.

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
