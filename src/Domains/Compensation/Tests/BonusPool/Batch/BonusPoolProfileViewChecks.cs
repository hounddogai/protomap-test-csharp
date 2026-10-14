namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolProfileViewChecks
{
    public static void BonusPoolProfileViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolProfile.NewBonusPoolProfile("code-273", "name-914");
        var view = BonusPoolProfileView.ToBonusPoolProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolProfileView.ToBonusPoolProfileViews(new List<BonusPoolProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolProfileViewShowsTheStatusInCapitals();
    }
}
