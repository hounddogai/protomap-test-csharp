namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanThresholdViewChecks
{
    public static void AdaptiveReturnPlanThresholdViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("code-509", "label-991");
        var view = AdaptiveReturnPlanThresholdView.ToAdaptiveReturnPlanThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveReturnPlanThresholdView.ToAdaptiveReturnPlanThresholdViews(new List<AdaptiveReturnPlanThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveReturnPlanThresholdViewShowsTheStatusInCapitals();
    }
}
