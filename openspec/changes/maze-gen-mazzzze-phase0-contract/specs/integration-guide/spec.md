## ADDED Requirements

### Requirement: A living integration guide covers every scenario with current status

The project SHALL maintain `docs/INTEGRATION.md` as the single, always-latest integration guide. It MUST be organized by the runtime actors in `docs/SCENARIOS.md` (game developer, game server, in-game client) and MUST list every scenario defined in `docs/SCENARIOS.md` with its current support status. For a supported scenario it MUST describe how to integrate it against the current public API; for a not-yet-supported scenario it MUST include a stub marking the phase it is planned for, rather than omitting it.

#### Scenario: Every scenario is present with a status
- **WHEN** a reader opens `docs/INTEGRATION.md`
- **THEN** every scenario ID in `docs/SCENARIOS.md` (D1–D6, S1–S7, C1–C5) appears with a current support status (supported / partial / planned + phase)

#### Scenario: Not-yet-integratable scenarios are stubbed, not dropped
- **WHEN** a scenario has no current integration path
- **THEN** the guide lists it as a "planned — Phase N" stub so coverage is complete by construction

#### Scenario: A supported scenario carries an integration recipe
- **WHEN** a scenario is marked supported
- **THEN** the guide shows how to integrate it against the current public API (the skeleton may defer recipe prose, but the section exists)

### Requirement: Guide upkeep is enforced by an OpenSpec config rule

Maintenance of the guide SHALL be enforced by a rule in `openspec/config.yaml` rather than by roadmap prose, so the obligation is auto-injected into every future change's artifact instructions. Any change that alters the public API or changes a scenario's support status MUST update `docs/INTEGRATION.md` and re-mark the affected `docs/SCENARIOS.md` rows within the same change.

#### Scenario: The rule surfaces in future change instructions
- **WHEN** an author generates task instructions for any future change (`openspec instructions tasks`)
- **THEN** the config rule reminding them to update `docs/INTEGRATION.md` for contract or status changes appears in those instructions

#### Scenario: A contract-altering change updates the guide
- **WHEN** a change adds, modifies, or removes public API, or changes a scenario's support status
- **THEN** the same change updates `docs/INTEGRATION.md` and the affected `docs/SCENARIOS.md` status rows
