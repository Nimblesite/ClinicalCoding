---
name: spec-check
description: Audit spec/plan documents against the codebase. Ensures every spec section has implementing code, tests, and matching logic. Use when the user says "check specs", "spec audit", or "verify specs".
argument-hint: "[optional spec ID or filename filter]"
---
<!-- agent-pmo:d58c330 -->

# spec-check

> **Portable skill.** This skill adapts to the current repository. The agent MUST inspect the repo structure and use judgment to apply these instructions appropriately.

Audit spec/plan documents against the codebase. Ensures every spec section has implementing code, tests, and that the code logic matches the spec.

## Arguments

- `$ARGUMENTS` -- optional spec name or ID to check (e.g., `AUTH-TOKEN-VERIFY` or `repo-standards`). If empty, check ALL specs. Spec IDs are descriptive slugs, NEVER numbered (see Step 1).

## Instructions

Follow these steps exactly. Be strict and pedantic. Stop on the first failure.

---

### Step 1: Validate spec ID structure

Before checking code/test references, verify that the specs themselves are well-formed.

1. Find all spec documents (see locations in Step 2).
2. Extract every section ID using the regex `\[([A-Z][A-Z0-9]*(-[A-Z0-9]+)+)\]`.
3. **Flag invalid IDs:**
   - Numbered IDs (`[SPEC-001]`, `[REQ-003]`, `[CI-004]`) -- must be renamed to descriptive hierarchical slugs.
   - Single-word IDs (`[TIMEOUT]`) -- must have a group prefix.
   - IDs with trailing numbers (`[FEAT-AUTH-01]`) -- the number is meaningless, remove it.
4. **Check group clustering:** The first word of each ID is its group. All sections in the same group MUST appear together (adjacent) in the document. If they're scattered, flag it.
5. **Check for missing IDs:** Any heading that defines a requirement or behavior should have an ID. Flag headings in spec files that look like they define behavior but lack an ID.

If any ID violations are found, report them all and **STOP**.

If all IDs are valid, proceed to Step 2.

---

### Step 2: Find all spec/plan documents

Search for markdown files that contain spec sections with IDs. Look in these locations:

- `docs/*.md`
- `docs/**/*.md`
- `SPEC.md`
- `PLAN.md`
- `specs/*.md`

Use Glob to find candidate files, then use Grep to confirm they contain spec IDs.

**Spec ID patterns** -- IDs appear in square brackets, typically at the start of a heading or section line. Match this regex pattern:

```
\[([A-Z][A-Z0-9]*(-[A-Z0-9]+)+)\]
```

Spec IDs are **hierarchical descriptive slugs, NEVER numbered.** The format is `[GROUP-TOPIC]` or `[GROUP-TOPIC-DETAIL]`.

For each file, extract every spec ID and its associated section title and full section content.

---

### Step 3: Filter specs

- If `$ARGUMENTS` is non-empty, filter the discovered specs.
- If `$ARGUMENTS` is empty, process ALL discovered specs.

---

### Step 4: Check each spec section

For EACH spec section that has an ID, perform checks A, B, and C below. **Stop on the first failure.**

#### Check A: Code references the spec ID

Search the entire codebase for the spec ID string, **excluding** `docs/`, `node_modules/`, `.git/`, and `*.md` files.

Any comment containing the exact spec ID string counts as a valid code reference.

**If NO code files reference the spec ID:**

```
SPEC VIOLATION: [ID] "Section Title" has no implementing code.

ACTION REQUIRED: Add a comment referencing [ID] in the file(s) that implement
this spec section, then re-run spec-check.
```

**STOP HERE.**

#### Check B: Tests reference the spec ID

Search test files (`**/*.Tests/**`, `**/*Tests.*`, `**/*Test.*`) for the literal spec ID string.

**If NO test files reference the spec ID:**

```
SPEC VIOLATION: [ID] "Section Title" has no tests.

ACTION REQUIRED: Add tests for [ID] with a comment or test name containing
the spec ID, then re-run spec-check.
```

**STOP HERE.**

#### Check C: Code logic matches the spec

1. Read the spec section content carefully.
2. Read the implementing code.
3. Compare spec vs. code. Be SENSITIVE and PEDANTIC. Check for ordering violations, missing conditions, wrong logic, missing steps, wrong defaults.
4. If the code deviates from the spec, report a detailed error with quotes from both spec and code.

**STOP HERE on any deviation.**

5. If the code matches the spec, this check passes. Move to the next spec.

---

### Step 5: Report results

On failure: output ONLY the first violation found.

On success: output a summary table of all specs checked.

## Key principles

- **Fail fast.** Stop on the first violation. One fix at a time.
- **Be pedantic.** If the spec says it, the code must do it.
- **Quote everything.** Always quote the spec text and the code in error messages.
- **Be actionable.** Every error must tell the developer what file to change and what to do.
- **No numbered IDs.** Spec IDs are hierarchical descriptive slugs, NEVER sequential numbers.
