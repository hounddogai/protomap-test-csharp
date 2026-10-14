namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedLicenseSeatCheckpointChecks
{
    public static void CappedLicenseSeatCheckpointStartsInNew()
    {
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("code-948", "title-530");
        Check.Equal(item.Status, CappedLicenseSeatCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedLicenseSeatCheckpointMovesToQueued()
    {
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("code-948", "title-530");
        try
        {
            item.MoveTo(CappedLicenseSeatCheckpointStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedLicenseSeatCheckpointStatus.Queued, "status");
    }

    public static void CappedLicenseSeatCheckpointRejectsMovesFromFailed()
    {
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("code-948", "title-530");
        item.Status = CappedLicenseSeatCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedLicenseSeatCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedLicenseSeatCheckpointStartsInNew();
        CappedLicenseSeatCheckpointMovesToQueued();
        CappedLicenseSeatCheckpointRejectsMovesFromFailed();
    }
}
