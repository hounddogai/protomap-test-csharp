namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanDraftViewChecks
{
    public static void SuccessionPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("code-326", "title-862");
        var view = SuccessionPlanDraftView.ToSuccessionPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanDraftView.ToSuccessionPlanDraftViews(new List<SuccessionPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanDraftViewShowsTheStatusInCapitals();
    }
}
