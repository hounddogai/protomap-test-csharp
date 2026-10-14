namespace Acme.Workforce.Payrollops.Tests.RetroPay.Summary;

using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RollingRetroPaySummaryChecks
{
    public static void RollingRetroPaySummaryStartsInPlanned()
    {
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("code-866", "name-490");
        Check.Equal(item.Status, RollingRetroPaySummaryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRetroPaySummaryMovesToScheduled()
    {
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("code-866", "name-490");
        try
        {
            item.MoveTo(RollingRetroPaySummaryStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRetroPaySummaryStatus.Scheduled, "status");
    }

    public static void RollingRetroPaySummaryRejectsMovesFromCancelled()
    {
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("code-866", "name-490");
        item.Status = RollingRetroPaySummaryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingRetroPaySummaryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingRetroPaySummaryStartsInPlanned();
        RollingRetroPaySummaryMovesToScheduled();
        RollingRetroPaySummaryRejectsMovesFromCancelled();
    }
}
