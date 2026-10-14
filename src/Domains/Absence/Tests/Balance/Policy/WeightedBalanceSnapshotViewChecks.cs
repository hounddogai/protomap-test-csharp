namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class WeightedBalanceSnapshotViewChecks
{
    public static void WeightedBalanceSnapshotViewShowsTheStatusInCapitals()
    {
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("code-519", "label-845");
        var view = WeightedBalanceSnapshotView.ToWeightedBalanceSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedBalanceSnapshotView.ToWeightedBalanceSnapshotViews(new List<WeightedBalanceSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedBalanceSnapshotViewShowsTheStatusInCapitals();
    }
}
