namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedLicenseSeatBundleViewChecks
{
    public static void WeightedLicenseSeatBundleViewShowsTheStatusInCapitals()
    {
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("code-344", "label-691");
        var view = WeightedLicenseSeatBundleView.ToWeightedLicenseSeatBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedLicenseSeatBundleView.ToWeightedLicenseSeatBundleViews(new List<WeightedLicenseSeatBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatBundleViewShowsTheStatusInCapitals();
    }
}
