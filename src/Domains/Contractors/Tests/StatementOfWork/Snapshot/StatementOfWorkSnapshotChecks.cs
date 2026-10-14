namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkSnapshotChecks
{
    public static void StatementOfWorkSnapshotStartsInCreated()
    {
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("code-360", "label-601");
        Check.Equal(item.Status, StatementOfWorkSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkSnapshotMovesToPublished()
    {
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("code-360", "label-601");
        try
        {
            item.MoveTo(StatementOfWorkSnapshotStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkSnapshotStatus.Published, "status");
    }

    public static void StatementOfWorkSnapshotRejectsMovesFromRetired()
    {
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("code-360", "label-601");
        item.Status = StatementOfWorkSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(StatementOfWorkSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkSnapshotStartsInCreated();
        StatementOfWorkSnapshotMovesToPublished();
        StatementOfWorkSnapshotRejectsMovesFromRetired();
    }
}
