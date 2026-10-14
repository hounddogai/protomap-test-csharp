namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentPreferenceViewChecks
{
    public static void AdjustmentPreferenceViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentPreference.NewAdjustmentPreference("code-452", "label-944");
        var view = AdjustmentPreferenceView.ToAdjustmentPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentPreferenceView.ToAdjustmentPreferenceViews(new List<AdjustmentPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentPreferenceViewShowsTheStatusInCapitals();
    }
}
