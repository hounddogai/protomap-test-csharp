namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class SuccessionPlanProposalViewChecks
{
    public static void SuccessionPlanProposalViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("code-566", "name-505");
        var view = SuccessionPlanProposalView.ToSuccessionPlanProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanProposalView.ToSuccessionPlanProposalViews(new List<SuccessionPlanProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanProposalViewShowsTheStatusInCapitals();
    }
}
