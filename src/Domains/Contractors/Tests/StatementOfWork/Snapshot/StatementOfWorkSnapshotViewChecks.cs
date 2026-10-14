namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkSnapshotViewChecks
{
    public static void StatementOfWorkSnapshotViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("code-360", "label-601");
        var view = StatementOfWorkSnapshotView.ToStatementOfWorkSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkSnapshotView.ToStatementOfWorkSnapshotViews(new List<StatementOfWorkSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkSnapshotViewShowsTheStatusInCapitals();
    }
}
