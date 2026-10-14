namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationMilestoneEngineChecks
{
    public static void NominationMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = NominationMilestoneEngine.DefaultNominationMilestoneEngine(Flags.NewRegistry());
        var item = NominationMilestone.NewNominationMilestone("code-82", "title-722");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationMilestoneEngineKeepsResultsWithinBounds();
    }
}
