namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class WeightedLaptopEstimateViewChecks
{
    public static void WeightedLaptopEstimateViewShowsTheStatusInCapitals()
    {
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("code-152", "title-599");
        var view = WeightedLaptopEstimateView.ToWeightedLaptopEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedLaptopEstimateView.ToWeightedLaptopEstimateViews(new List<WeightedLaptopEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedLaptopEstimateViewShowsTheStatusInCapitals();
    }
}
