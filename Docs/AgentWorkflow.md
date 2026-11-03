# Shared Issue workflow for Codex and Claude

## Scope and shared record

Use GitHub Issues for each bounded development, research, hardware, or
manufacturing task. The main repository is
<https://github.com/AtsushiHosaka/ChopChop>. The shared private Project is
<https://github.com/users/AtsushiHosaka/projects/2> (Project number 2).
Search existing Issues and Projects
before creating anything, and continue the existing record when it covers the
same outcome. Use an Issue in another repository when that repository owns the
work, and link it from the ChopChop tracking Issue when needed.

Keep current scope, acceptance criteria, and the latest status in the Issue
body. Keep dated progress and decision history in comments. Link implementation
PRs, commits, specifications, and evidence. GitHub Projects provides a shared
board view of those Issues. Inspect its actual fields and options before using
them; do not invent a Project URL, number, status option, or completed update.
The Project's exact status options must be inspected before updating it. If a
Status field is absent or an option is unavailable, keep the exact status and
owner in the Issue body and add a dated status comment. Do not report a Project
status update as completed when only the Issue text was updated.

## Before starting

1. Read the Issue body, recent comments, user decisions, linked specifications,
   and current implementation. Recheck changeable claims in their owning source.
2. Confirm the intended outcome and acceptance criteria. Identify dependencies,
   authorization limits, and files or applications shared with another agent.
3. Claim the work in an Issue comment before editing. Include the agent name
   (Codex or Claude), date/time with timezone, chat/task link when available,
   intended scope, and branch/worktree. Re-read recent comments after posting
   the claim; a comment is not an atomic lock. Coordinate if another agent owns
   overlapping work. Do not take over merely because its last comment is old.
4. Move the Issue to In Progress only when work actually begins. Use a branch
   that identifies the Issue when practical. Do not initialize or migrate the
   whole workspace repository, copy applications, or replace user changes as
   part of routine task setup.

## Status meanings

| Status | Meaning |
| --- | --- |
| Todo | Scope and next action are recorded; work has not started. |
| In Progress | A named agent is actively working on the recorded scope. |
| Blocked | Work cannot continue without a stated dependency or user decision; record the exact unblock condition. |
| In Review | A concrete result is ready with evidence; name the required review or acceptance action. |
| Done | Acceptance criteria are met and verified; remaining work is explicitly recorded elsewhere. |

Map these meanings to existing Project options rather than creating duplicate
fields or boards. If an exact option is unavailable, record the precise status
in the Issue body/comment and state the board mapping. Do not close an Issue or
mark Done solely because a PR was opened, code compiled, an email was sent, or
one stage of a multi-stage task finished. Reopen or correct the status if new
evidence invalidates completion.

## User decisions

Record each material user decision promptly in the owning Issue. Include:

- Date/time and timezone, the user as decision-maker, and the source chat or
  other accessible evidence link. If no link is available, say so and include
  a faithful short summary of the instruction.
- The question, chosen behavior or option, reason when the user supplied one,
  affected scope, constraints, and any explicitly rejected alternative.
- Whether this is a user instruction, agent proposal, unresolved question, or
  verified finding. Do not present agent inference as a user decision.
- The earlier decision it supersedes, if any. Preserve the old record and add
  a correction instead of silently rewriting history.

Record decisions that affect requirements, component selection, CAD, acceptance
criteria, purchases, production, deployment, or merging. A review request is
not purchase or production authorization. Preserve the user's actual approval
scope and conditions; do not add a new approval gate for actions already
authorized. Do not copy credentials, private addresses, payment information,
or entire private conversations into GitHub. Summarize only what is necessary
and appropriate for the repository's visibility.

## Progress, evidence, and handoff

Update the Issue after a material finding, scope change, blocker, review-ready
result, or before ending/handoff. A useful update contains:

```text
Time / agent / task link:
Status and owner:
Current scope and acceptance criteria:
User decisions since the last update:
Changes: branch, commit/PR, relevant files or artifact identifiers
Verified: checks, environment/device, result, evidence links
Unverified or blocked: exact limitation and unblock condition
Next action and owner:
```

Separate facts from proposals and completed actions from intended actions.
For hardware, identify the design revision and actual order/part number;
distinguish CAD checks, vendor data, production files, and physical-device
measurements. A manufacturing approval email does not prove the website's
approval state or completed assembly. For software, follow the Demo/Production
separation in `DEVELOPMENT_RULES.md`.

Use repository-relative paths for tracked evidence and stable URLs for shared
artifacts. For files outside a repository, record their location, revision/hash
when relevant, and availability to the next agent; do not assume an absolute
local path is accessible to another checkout. Make no claim that such files
were committed unless they actually were.

Before handing off, name what remains and whether another agent may take over.
The receiving agent reads the Issue and verifies the working tree before
continuing. Avoid duplicating an active task in a second chat or branch.

## When GitHub is unavailable

State the failed operation and do not claim it succeeded. Continue independent
read-only checks and already-authorized local work that does not conflict with
another owner. Preserve a pending update with the Issue URL when known, exact
decision/evidence text, and next action in the task's local notes. Sync it to
GitHub when access returns and verify the resulting state. Local notes are a
temporary fallback, not a second source of truth.
