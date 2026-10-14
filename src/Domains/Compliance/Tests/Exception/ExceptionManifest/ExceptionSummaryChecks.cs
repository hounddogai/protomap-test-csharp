namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ExceptionSummaryChecks
{
    public static void ExceptionSummaryStartsInIdle()
    {
        var item = ExceptionSummary.NewExceptionSummary("code-451", "name-529");
        Check.Equal(item.Status, ExceptionSummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionSummaryMovesToDraining()
    {
        var item = ExceptionSummary.NewExceptionSummary("code-451", "name-529");
        try
        {
            item.MoveTo(ExceptionSummaryStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionSummaryStatus.Draining, "status");
    }

    public static void ExceptionSummaryRejectsMovesFromStopped()
    {
        var item = ExceptionSummary.NewExceptionSummary("code-451", "name-529");
        item.Status = ExceptionSummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(ExceptionSummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ExceptionSummaryStartsInIdle();
        ExceptionSummaryMovesToDraining();
        ExceptionSummaryRejectsMovesFromStopped();
    }
}
