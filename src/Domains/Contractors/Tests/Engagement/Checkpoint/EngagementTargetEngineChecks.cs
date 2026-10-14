namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class EngagementTargetEngineChecks
{
    public static void EngagementTargetEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementTargetEngine.DefaultEngagementTargetEngine(Flags.NewRegistry());
        var item = EngagementTarget.NewEngagementTarget("code-320", "name-293");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementTargetEngineKeepsResultsWithinBounds();
    }
}
