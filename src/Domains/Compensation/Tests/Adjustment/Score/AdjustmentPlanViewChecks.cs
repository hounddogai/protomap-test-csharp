namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentPlanViewChecks
{
    public static void AdjustmentPlanViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentPlan.NewAdjustmentPlan("code-836", "label-183");
        var view = AdjustmentPlanView.ToAdjustmentPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentPlanView.ToAdjustmentPlanViews(new List<AdjustmentPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentPlanViewShowsTheStatusInCapitals();
    }
}
