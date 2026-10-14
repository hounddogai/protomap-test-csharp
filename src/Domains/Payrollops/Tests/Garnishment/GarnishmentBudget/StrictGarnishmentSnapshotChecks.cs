namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class StrictGarnishmentSnapshotChecks
{
    public static void StrictGarnishmentSnapshotStartsInNew()
    {
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("code-707", "title-431");
        Check.Equal(item.Status, StrictGarnishmentSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictGarnishmentSnapshotMovesToDone()
    {
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("code-707", "title-431");
        try
        {
            item.MoveTo(StrictGarnishmentSnapshotStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictGarnishmentSnapshotStatus.Done, "status");
    }

    public static void StrictGarnishmentSnapshotRejectsMovesFromFailed()
    {
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("code-707", "title-431");
        item.Status = StrictGarnishmentSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictGarnishmentSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictGarnishmentSnapshotStartsInNew();
        StrictGarnishmentSnapshotMovesToDone();
        StrictGarnishmentSnapshotRejectsMovesFromFailed();
    }
}
