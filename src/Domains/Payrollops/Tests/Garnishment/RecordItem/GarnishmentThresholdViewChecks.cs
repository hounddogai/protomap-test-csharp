namespace Acme.Workforce.Payrollops.Tests.Garnishment.RecordItem;

using Acme.Workforce.Payrollops.Garnishment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GarnishmentThresholdViewChecks
{
    public static void GarnishmentThresholdViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentThreshold.NewGarnishmentThreshold("code-541", "label-29");
        var view = GarnishmentThresholdView.ToGarnishmentThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentThresholdView.ToGarnishmentThresholdViews(new List<GarnishmentThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentThresholdViewShowsTheStatusInCapitals();
    }
}
