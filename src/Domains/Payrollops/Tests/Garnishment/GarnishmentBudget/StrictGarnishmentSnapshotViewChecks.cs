namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class StrictGarnishmentSnapshotViewChecks
{
    public static void StrictGarnishmentSnapshotViewShowsTheStatusInCapitals()
    {
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("code-707", "title-431");
        var view = StrictGarnishmentSnapshotView.ToStrictGarnishmentSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictGarnishmentSnapshotView.ToStrictGarnishmentSnapshotViews(new List<StrictGarnishmentSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictGarnishmentSnapshotViewShowsTheStatusInCapitals();
    }
}
