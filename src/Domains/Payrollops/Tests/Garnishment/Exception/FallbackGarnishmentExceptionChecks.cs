namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class FallbackGarnishmentExceptionChecks
{
    public static void FallbackGarnishmentExceptionStartsInIdle()
    {
        var item = FallbackGarnishmentException.NewFallbackGarnishmentException("code-942", "name-868");
        Check.Equal(item.Status, FallbackGarnishmentExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackGarnishmentExceptionMovesToWarming()
    {
        var item = FallbackGarnishmentException.NewFallbackGarnishmentException("code-942", "name-868");
        try
        {
            item.MoveTo(FallbackGarnishmentExceptionStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackGarnishmentExceptionStatus.Warming, "status");
    }

    public static void FallbackGarnishmentExceptionRejectsMovesFromStopped()
    {
        var item = FallbackGarnishmentException.NewFallbackGarnishmentException("code-942", "name-868");
        item.Status = FallbackGarnishmentExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackGarnishmentExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackGarnishmentExceptionStartsInIdle();
        FallbackGarnishmentExceptionMovesToWarming();
        FallbackGarnishmentExceptionRejectsMovesFromStopped();
    }
}
