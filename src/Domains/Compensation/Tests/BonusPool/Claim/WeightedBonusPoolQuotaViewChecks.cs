namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class WeightedBonusPoolQuotaViewChecks
{
    public static void WeightedBonusPoolQuotaViewShowsTheStatusInCapitals()
    {
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("code-939", "name-579");
        var view = WeightedBonusPoolQuotaView.ToWeightedBonusPoolQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedBonusPoolQuotaView.ToWeightedBonusPoolQuotaViews(new List<WeightedBonusPoolQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedBonusPoolQuotaViewShowsTheStatusInCapitals();
    }
}
