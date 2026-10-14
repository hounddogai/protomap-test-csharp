namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentExceptionChecks
{
    public static void GarnishmentExceptionStartsInNew()
    {
        var item = GarnishmentException.NewGarnishmentException("code-291", "title-256");
        Check.Equal(item.Status, GarnishmentExceptionStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentExceptionMovesToDone()
    {
        var item = GarnishmentException.NewGarnishmentException("code-291", "title-256");
        try
        {
            item.MoveTo(GarnishmentExceptionStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentExceptionStatus.Done, "status");
    }

    public static void GarnishmentExceptionRejectsMovesFromFailed()
    {
        var item = GarnishmentException.NewGarnishmentException("code-291", "title-256");
        item.Status = GarnishmentExceptionStatus.Failed;
        Check.Fails(() => item.MoveTo(GarnishmentExceptionStatus.New), "move");
    }

    public static void RunAll()
    {
        GarnishmentExceptionStartsInNew();
        GarnishmentExceptionMovesToDone();
        GarnishmentExceptionRejectsMovesFromFailed();
    }
}
