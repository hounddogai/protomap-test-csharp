namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class SuccessionPlanProposalEngineChecks
{
    public static void SuccessionPlanProposalEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanProposalEngine.DefaultSuccessionPlanProposalEngine(Flags.NewRegistry());
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("code-566", "name-505");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanProposalEngineKeepsResultsWithinBounds();
    }
}
