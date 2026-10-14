namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class LegacyBonusPoolAllocationViewChecks
{
    public static void LegacyBonusPoolAllocationViewShowsTheStatusInCapitals()
    {
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("code-296", "title-345");
        var view = LegacyBonusPoolAllocationView.ToLegacyBonusPoolAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyBonusPoolAllocationView.ToLegacyBonusPoolAllocationViews(new List<LegacyBonusPoolAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyBonusPoolAllocationViewShowsTheStatusInCapitals();
    }
}
