# Design Decisions - Team Leave Scheduler

### 1. The 30% Rule (Rounding)
**Decision:** I chose to use `Math.Round()` but implemented a **minimum floor of 1 person**.
**Reasoning:** In a team of 4, 30% is 1.2. Rounding to the nearest whole number gives 1. However, for a team of 2, 30% is 0.6, which would round to 1. If I had used "Floor," a team of 2 would never be allowed to take leave (0.6 rounds down to 0). Setting a minimum of 1 person ensures that even micro-teams can function while maintaining business continuity.

### 2. Multi-Day Requests
**Decision:** Evaluation is performed **per-day**.
**Reasoning:** If an employee requests a week off, but Wednesday already has too many people away, the entire request should be rejected. This prevents "hidden" capacity violations that might occur if we only checked the start or end dates.

### 3. Weekends and Public Holidays
**Decision:** These are automatically "skipped" by the validation engine.
**Reasoning:** The business rule states weekends are not working days and holidays don't count against balance. My system filters these out before checking team capacity. This means if a team is at 30% capacity on a Friday and Monday, but there is a holiday on the Monday, the request is still valid for the Friday because the holiday doesn't "count" toward the limit.

### 4. Overlapping Requests
**Decision:** Invalid only if they overlap an **Approved** request.
**Reasoning:** To prevent double-booking, the system blocks any new submission that conflicts with an already approved slot. If two requests are "Pending" for the same time, the manager has the discretion to approve one, which then makes the other potentially invalid upon approval attempt.