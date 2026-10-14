namespace Acme.Workforce.Absence.Tests.ReturnPlan.Audit;

using Acme.Workforce.Absence.ReturnPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ReturnPlanDraftViewChecks
{
    public static void ReturnPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanDraft.NewReturnPlanDraft("code-864", "label-671");
        var view = ReturnPlanDraftView.ToReturnPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanDraftView.ToReturnPlanDraftViews(new List<ReturnPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanDraftViewShowsTheStatusInCapitals();
    }
}
