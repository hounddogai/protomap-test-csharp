namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanDraftViewChecks
{
    public static void RegionalSuccessionPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("code-489", "title-653");
        var view = RegionalSuccessionPlanDraftView.ToRegionalSuccessionPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalSuccessionPlanDraftView.ToRegionalSuccessionPlanDraftViews(new List<RegionalSuccessionPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanDraftViewShowsTheStatusInCapitals();
    }
}
