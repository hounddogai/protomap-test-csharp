namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementProfileEngineChecks
{
    public static void EngagementProfileEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementProfileEngine.DefaultEngagementProfileEngine(Flags.NewRegistry());
        var item = EngagementProfile.NewEngagementProfile("code-277", "name-353");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementProfileEngineKeepsResultsWithinBounds();
    }
}
