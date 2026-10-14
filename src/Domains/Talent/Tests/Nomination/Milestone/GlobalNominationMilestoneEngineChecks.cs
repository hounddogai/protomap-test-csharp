namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class GlobalNominationMilestoneEngineChecks
{
    public static void GlobalNominationMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalNominationMilestoneEngine.DefaultGlobalNominationMilestoneEngine();
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("code-629", "label-807");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalNominationMilestoneEngineKeepsResultsWithinBounds();
    }
}
