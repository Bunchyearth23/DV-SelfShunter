:: IMPORTANT ::
The code you are about to witness is an absolute cluster fuck. I would not wish it on anyone to go though the steaming heap of shit that is this repository. PEAPLE HAVE PERISHED HERE BEFOR! Let it be know that following B100, all code here shall be deemed irrelevant and quickly forgotten! YOU HAVE BEEN WARNED!

:: REQUIREMENTS ::
• Derail Valley

:: FEATURES ::
• Double Track added to the valley

:: GENERAL OTHER STUFF ::
• Works with Multi-player Mod and Remote Dispatch

::  KNOWN ISSUES ::
• The code is a cluster fuck
• Editing the targets file is not supported, here be dragons!

::  CREDITS Template ::
♦ Chump_the_Lump

BDVM integration fork
=====================

Original repository: https://github.com/Chump-the-Lump/DV-SelfShunter
Base revision: 329c85cf51715404af3b4d455239d9fc54f5ac5b

Credit: Chump_the_Lump

Reliability and BDVM integration
================================

The host is the sole authority for job generation, natural rolling-stock
population, consist assignment, lifecycle publication and cleanup. Clients only
receive replicated state. SelfShunt does not create an AI driver or move trains;
the player remains responsible for switching and hauling.

SelfShunt.API remains API version 1 for BDVM.SelfShuntBridge compatibility. Its
generation and population commands are host-only and idempotent. Lifecycle
events are observational: they never credit an account. BDVM remains the only
economic authority whenever its strict economy policy is active.

See UNITY_TESTS.md for the required in-game validation. The game is never
launched by the build or test process.
