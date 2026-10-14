namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultBonusPoolAllocationViewChecks
{
    public static void DefaultBonusPoolAllocationViewShowsTheStatusInCapitals()
    {
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("code-62", "name-131");
        var view = DefaultBonusPoolAllocationView.ToDefaultBonusPoolAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultBonusPoolAllocationView.ToDefaultBonusPoolAllocationViews(new List<DefaultBonusPoolAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultBonusPoolAllocationViewShowsTheStatusInCapitals();
    }
}
