namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryAdjustmentPreferenceViewChecks
{
    public static void PrimaryAdjustmentPreferenceViewShowsTheStatusInCapitals()
    {
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("code-995", "label-564");
        var view = PrimaryAdjustmentPreferenceView.ToPrimaryAdjustmentPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryAdjustmentPreferenceView.ToPrimaryAdjustmentPreferenceViews(new List<PrimaryAdjustmentPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryAdjustmentPreferenceViewShowsTheStatusInCapitals();
    }
}
