# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Component architecture and delivery planning | Danny | Cross-project changes, trade-offs, code review, issue triage |
| Tag helpers, Razor integration, server-side behavior | Rusty | Component rendering, HTML generation, ASP.NET Core integration |
| CSS, JavaScript, and demo UX | Basher | Component styling, interactions, layout asset wiring |
| Test coverage and regression hunting | Livingston | Unit tests, Playwright, edge cases, bug reproduction |
| Docs pages, examples, and contributor guidance | Linus | Demo/docs authoring, examples, onboarding content |
| Code review | Danny | Review PRs, check quality, suggest improvements |
| Testing | Livingston | Write tests, find edge cases, verify fixes |
| Scope & priorities | Danny | What to build next, trade-offs, decisions |
| Session logging | Scribe | Automatic — never needs routing |
| RAI review | Rai | Content safety, bias checks, credential detection, ethical review |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Danny |
| `squad:danny` | Pick up issue and complete the work | Danny |
| `squad:rusty` | Pick up issue and complete the work | Rusty |
| `squad:basher` | Pick up issue and complete the work | Basher |
| `squad:livingston` | Pick up issue and complete the work | Livingston |
| `squad:linus` | Pick up issue and complete the work | Linus |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, the **Lead** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Lead review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn the tester to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. The Lead handles all `squad` (base label) triage.
