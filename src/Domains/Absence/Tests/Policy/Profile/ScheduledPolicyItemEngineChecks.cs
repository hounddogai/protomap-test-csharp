namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ScheduledPolicyItemEngineChecks
{
    public static void ScheduledPolicyItemEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPolicyItemEngine.DefaultScheduledPolicyItemEngine(Flags.NewRegistry());
        var item = ScheduledPolicyItem.NewScheduledPolicyItem("code-967", "title-667");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPolicyItemEngineKeepsResultsWithinBounds();
    }
}
