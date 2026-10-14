namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeskBatchChecks
{
    public static void DeskBatchStartsInIdle()
    {
        var item = DeskBatch.NewDeskBatch("code-179", "name-378");
        Check.Equal(item.Status, DeskBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskBatchMovesToWarming()
    {
        var item = DeskBatch.NewDeskBatch("code-179", "name-378");
        try
        {
            item.MoveTo(DeskBatchStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskBatchStatus.Warming, "status");
    }

    public static void DeskBatchRejectsMovesFromStopped()
    {
        var item = DeskBatch.NewDeskBatch("code-179", "name-378");
        item.Status = DeskBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeskBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeskBatchStartsInIdle();
        DeskBatchMovesToWarming();
        DeskBatchRejectsMovesFromStopped();
    }
}
