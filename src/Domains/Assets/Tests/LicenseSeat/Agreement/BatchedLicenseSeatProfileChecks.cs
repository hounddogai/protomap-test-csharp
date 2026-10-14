namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class BatchedLicenseSeatProfileChecks
{
    public static void BatchedLicenseSeatProfileStartsInDraft()
    {
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("code-88", "name-597");
        Check.Equal(item.Status, BatchedLicenseSeatProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedLicenseSeatProfileMovesToActive()
    {
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("code-88", "name-597");
        try
        {
            item.MoveTo(BatchedLicenseSeatProfileStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedLicenseSeatProfileStatus.Active, "status");
    }

    public static void BatchedLicenseSeatProfileRejectsMovesFromClosed()
    {
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("code-88", "name-597");
        item.Status = BatchedLicenseSeatProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedLicenseSeatProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedLicenseSeatProfileStartsInDraft();
        BatchedLicenseSeatProfileMovesToActive();
        BatchedLicenseSeatProfileRejectsMovesFromClosed();
    }
}
