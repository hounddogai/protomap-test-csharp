namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WeightedLicenseSeatEstimateViewChecks
{
    public static void WeightedLicenseSeatEstimateViewShowsTheStatusInCapitals()
    {
        var item = WeightedLicenseSeatEstimate.NewWeightedLicenseSeatEstimate("code-319", "name-685");
        var view = WeightedLicenseSeatEstimateView.ToWeightedLicenseSeatEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedLicenseSeatEstimateView.ToWeightedLicenseSeatEstimateViews(new List<WeightedLicenseSeatEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatEstimateViewShowsTheStatusInCapitals();
    }
}
