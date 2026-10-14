namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedHeadcountWindowViewChecks
{
    public static void WeightedHeadcountWindowViewShowsTheStatusInCapitals()
    {
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("code-729", "label-977");
        var view = WeightedHeadcountWindowView.ToWeightedHeadcountWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedHeadcountWindowView.ToWeightedHeadcountWindowViews(new List<WeightedHeadcountWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedHeadcountWindowViewShowsTheStatusInCapitals();
    }
}
