# AI Usage Log

### 1. Tools Used
*   **Google AI Studio:** Used for architectural guidance, business logic implementation, and localization and for minor syntax completions.

### 2. Most Useful Prompts
*   **Prompt 1 (Logic Definition):** "I need to implement a rule where no more than 30% of a team can be on leave on a specific day. How should I handle rounding for a team of 4, and how do I evaluate multi-day requests per-day in C#?"
    *   *Impact:* This helped me define the `LeaveService` logic, ensuring that micro-teams (1-2 people) aren't locked out of taking leave by setting a minimum cap of 1 person.
*   **Prompt 2 (Localization):** "Help me localize my test data. I need a CSV of 15 Zimbabwean names across three teams and a JSON file containing Zimbabwean national holidays for the second half of 2026."
    *   *Impact:* This allowed me to make the application more relatable for a local context while ensuring the business logic (skipping holidays) was tested against real dates.

### 3. A Case Where AI Got It Wrong
**The Error:** During the database setup, the AI suggested installing the latest version of Entity Framework Core (Version 10.0.9). 
**The Conflict:** My project was built using the .NET 8.0 SDK. The version 10 tools were incompatible with my SDK, leading to a series of `NU1202` compatibility errors during the build process.
**The Fix:** I had to manually override the installation to use Version 8.0.0 (`dotnet add package ... --version 8.0.0`) to match my development environment. This taught me to always verify that library versions match the core SDK version.

### 4. Human Oversight
While AI assisted with generating boilerplate code and structuring the `LeaveService`, I performed the following manual tasks:
*   Refining the folder structure to ensure a professional separation between API and UI.
*   Manually fixing namespace errors in `Program.cs` that occurred during code integration.
*   Interpreting the "ambiguous" 30% rule and making the final executive decision on rounding methods documented in `DECISIONS.md`.