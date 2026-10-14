namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationPlanEngineChecks
{
    public static void NominationPlanEngineKeepsResultsWithinBounds()
    {
        var engine = NominationPlanEngine.DefaultNominationPlanEngine(Flags.NewRegistry());
        var item = NominationPlan.NewNominationPlan("code-127", "label-252");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationPlanEngineKeepsResultsWithinBounds();
    }
}
