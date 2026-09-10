# SelfShunt / BDVM Unity validation

Automated compilation proves API and binary compatibility, but the following
scenarios require Derail Valley because jobs, warehouses, saves and multiplayer
authority are Unity runtime services. Do not treat a release as runtime-proven
until this checklist has been completed and its logs retained.

## Single-player lifecycle

1. Start a fresh sandbox save with SelfShunt only. Enter a station generation
   zone and confirm exactly one generation attempt per trigger, with no duplicate
   job ID or duplicate consist assignment in the log.
2. Accept a SelfShunt job, place the requested number of compatible empty cars on
   the loading track, load them, deliver them and complete the job. Confirm the
   booklet updates, plates are cleared at the terminal transition, and the cars
   return to vanilla cleanup eligibility.
3. Abandon a second job after loading. Confirm cargo and plates are cleared only
   for that job's cars; an unrelated freight car and caboose must retain vanilla
   behavior.
4. Save once while available, once while accepted but unassigned, and once after
   consist assignment. Reload each save and confirm a single job/definition,
   restored tasks and booklet, and no debt registration replay.
5. Temporarily invalidate a referenced warehouse or car in a disposable test
   save and confirm SelfShunt reports a precise refusal instead of constructing a
   partial job.

## Strict BDVM economy

1. Install matching builds of BDVM.Full, BDVM.SelfShuntBridge, SelfShunt and
   SelfShunt.API. Apply the strict policy on the host and confirm both generation
   and natural-car population report suspended.
2. Re-enter several station zones and wait beyond the population cadence. Confirm
   that SelfShunt creates neither jobs nor free rolling stock. Existing jobs must
   remain intact; the policy does not delete player work.
3. Replay the same operation ID and confirm no second policy event. Reuse that ID
   with different arguments and confirm refusal.
4. Disable the policy using a new operation ID and confirm controlled resumption
   without a generation burst or overlapping population coroutine.
5. Capture `JobCreated`, loading, delivery, completion and cancellation events.
   Confirm they are emitted once by the host and never mutate BDVM money. Verify
   all actual company credits in the BDVM ledger have BDVM idempotency keys.
6. Open the booklet for a BDVM-created SelfShunt job. Confirm it shows the
   planned base reward plus scarcity bonus with a `(BDVM)` suffix, while the
   SelfShunt/vanilla wage remains zero. Complete the job and confirm only BDVM
   credits the delivered amount and never more than the displayed total.

## Host/client

1. With two players, apply strict policy from the host, then attempt it from the
   client. The host must succeed and the client must fail closed.
2. Generate and accept a job on the host. Join late from the client and confirm a
   single matching overview, job ID, consist and `(BDVM)` display reward.
3. Load, deliver, abandon and reload in separate runs. Confirm only the host emits
   lifecycle/economic observations and the client never generates cars, assigns
   a consist, registers debt or performs terminal cleanup independently.

## Evidence to retain

- SelfShunt and MultiplayerAPI logs for every scenario.
- Save files before and after each reload scenario.
- BDVM ledger export showing no duplicate credit.
- Game, Unity Mod Manager, MultiplayerAPI, BDVM and mod versions plus the exact
  SelfShunt commit under test.
