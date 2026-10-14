namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantExceptionChecks
{
    public static void EquityGrantExceptionStartsInPlanned()
    {
        var item = EquityGrantException.NewEquityGrantException("code-711", "name-293");
        Check.Equal(item.Status, EquityGrantExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantExceptionMovesToInProgress()
    {
        var item = EquityGrantException.NewEquityGrantException("code-711", "name-293");
        try
        {
            item.MoveTo(EquityGrantExceptionStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantExceptionStatus.InProgress, "status");
    }

    public static void EquityGrantExceptionRejectsMovesFromCancelled()
    {
        var item = EquityGrantException.NewEquityGrantException("code-711", "name-293");
        item.Status = EquityGrantExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EquityGrantExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EquityGrantExceptionStartsInPlanned();
        EquityGrantExceptionMovesToInProgress();
        EquityGrantExceptionRejectsMovesFromCancelled();
    }
}
