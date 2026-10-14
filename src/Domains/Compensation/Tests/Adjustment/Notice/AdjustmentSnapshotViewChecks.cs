namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentSnapshotViewChecks
{
    public static void AdjustmentSnapshotViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("code-284", "label-925");
        var view = AdjustmentSnapshotView.ToAdjustmentSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentSnapshotView.ToAdjustmentSnapshotViews(new List<AdjustmentSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentSnapshotViewShowsTheStatusInCapitals();
    }
}
