SelfShunt - BDVM integration fork
=================================

SelfShunt creates local shunting work from compatible consists already present
in Derail Valley. It does not add an AI driver and never moves a train for the
player.

Requirements
============

- Derail Valley
- Unity Mod Manager
- Multiplayer is optional; when present, only the host owns generation and
  lifecycle authority.

BDVM integration
================

This branch adds SelfShunt.API for BDVM.SelfShuntBridge. The API exposes
host-only, idempotent controls for job generation and natural rolling-stock
population, plus observational lifecycle events. When strict BDVM economy is
enabled, SelfShunt wages are suppressed and BDVM remains the sole economic
authority. Clients only receive replicated state.

The integration fails closed when authority or a required capability cannot be
proved. Cleanup protection is limited to rolling stock assigned to an active
SelfShunt job; unrelated wagons and cabooses keep their normal game lifecycle.

Status
======

Version 1.1.0 is a beta candidate. Automated builds and policy tests pass, but
the Unity, save/reload and host/client campaign in UNITY_TESTS.md is required
before a stable release.

Credits and permission
======================

Original author and mandatory credit: Chump_the_Lump
Original repository: https://github.com/Chump-the-Lump/DV-SelfShunter
Fork repository: https://github.com/Bunchyearth23/DV-SelfShunter
Recorded base revision: 329c85cf51715404af3b4d455239d9fc54f5ac5b

The original author permits forks provided this credit, the original repository
link and the source revision remain visible. This permission is not represented
as a conventional open-source licence.

Build and validation
====================

Build SelfShunt.csproj and the API for net48 against a local Derail Valley
installation. No build or test command launches the game. Follow UNITY_TESTS.md
for the remaining runtime evidence.
