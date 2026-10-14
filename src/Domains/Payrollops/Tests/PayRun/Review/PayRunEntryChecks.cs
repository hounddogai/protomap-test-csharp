namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunEntryChecks
{
    public static void PayRunEntryStartsInNew()
    {
        var item = PayRunEntry.NewPayRunEntry("code-547", "title-683");
        Check.Equal(item.Status, PayRunEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunEntryMovesToRunning()
    {
        var item = PayRunEntry.NewPayRunEntry("code-547", "title-683");
        try
        {
            item.MoveTo(PayRunEntryStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunEntryStatus.Running, "status");
    }

    public static void PayRunEntryRejectsMovesFromFailed()
    {
        var item = PayRunEntry.NewPayRunEntry("code-547", "title-683");
        item.Status = PayRunEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(PayRunEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        PayRunEntryStartsInNew();
        PayRunEntryMovesToRunning();
        PayRunEntryRejectsMovesFromFailed();
    }
}
