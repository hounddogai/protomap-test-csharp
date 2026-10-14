namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedNominationMilestoneEngineChecks
{
    public static void WeightedNominationMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedNominationMilestoneEngine.DefaultWeightedNominationMilestoneEngine(Flags.NewRegistry());
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("code-77", "name-873");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedNominationMilestoneEngineKeepsResultsWithinBounds();
    }
}
