namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedBonusPoolVarianceViewChecks
{
    public static void WeightedBonusPoolVarianceViewShowsTheStatusInCapitals()
    {
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("code-936", "title-193");
        var view = WeightedBonusPoolVarianceView.ToWeightedBonusPoolVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedBonusPoolVarianceView.ToWeightedBonusPoolVarianceViews(new List<WeightedBonusPoolVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedBonusPoolVarianceViewShowsTheStatusInCapitals();
    }
}
