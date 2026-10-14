namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementPreferenceEngineChecks
{
    public static void EngagementPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementPreferenceEngine.DefaultEngagementPreferenceEngine(Flags.NewRegistry());
        var item = EngagementPreference.NewEngagementPreference("code-908", "name-996");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementPreferenceEngineKeepsResultsWithinBounds();
    }
}
