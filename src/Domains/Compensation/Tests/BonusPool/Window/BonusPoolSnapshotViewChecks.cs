namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolSnapshotViewChecks
{
    public static void BonusPoolSnapshotViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("code-567", "name-611");
        var view = BonusPoolSnapshotView.ToBonusPoolSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolSnapshotView.ToBonusPoolSnapshotViews(new List<BonusPoolSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolSnapshotViewShowsTheStatusInCapitals();
    }
}
