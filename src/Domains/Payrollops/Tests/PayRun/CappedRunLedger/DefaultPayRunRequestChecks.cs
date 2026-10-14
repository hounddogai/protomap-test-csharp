namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunRequestChecks
{
    public static void DefaultPayRunRequestStartsInPlanned()
    {
        var item = DefaultPayRunRequest.NewDefaultPayRunRequest("code-642", "label-204");
        Check.Equal(item.Status, DefaultPayRunRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayRunRequestMovesToScheduled()
    {
        var item = DefaultPayRunRequest.NewDefaultPayRunRequest("code-642", "label-204");
        try
        {
            item.MoveTo(DefaultPayRunRequestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayRunRequestStatus.Scheduled, "status");
    }

    public static void DefaultPayRunRequestRejectsMovesFromCancelled()
    {
        var item = DefaultPayRunRequest.NewDefaultPayRunRequest("code-642", "label-204");
        item.Status = DefaultPayRunRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultPayRunRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultPayRunRequestStartsInPlanned();
        DefaultPayRunRequestMovesToScheduled();
        DefaultPayRunRequestRejectsMovesFromCancelled();
    }
}
