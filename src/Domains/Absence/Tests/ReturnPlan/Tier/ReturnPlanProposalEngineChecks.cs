namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ReturnPlanProposalEngineChecks
{
    public static void ReturnPlanProposalEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanProposalEngine.DefaultReturnPlanProposalEngine(Flags.NewRegistry());
        var item = ReturnPlanProposal.NewReturnPlanProposal("code-734", "label-371");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanProposalEngineKeepsResultsWithinBounds();
    }
}
