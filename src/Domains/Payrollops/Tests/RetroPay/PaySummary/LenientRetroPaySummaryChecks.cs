namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class LenientRetroPaySummaryChecks
{
    public static void LenientRetroPaySummaryStartsInRequested()
    {
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("code-387", "name-362");
        Check.Equal(item.Status, LenientRetroPaySummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRetroPaySummaryMovesToGranted()
    {
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("code-387", "name-362");
        try
        {
            item.MoveTo(LenientRetroPaySummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRetroPaySummaryStatus.Granted, "status");
    }

    public static void LenientRetroPaySummaryRejectsMovesFromExpired()
    {
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("code-387", "name-362");
        item.Status = LenientRetroPaySummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientRetroPaySummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientRetroPaySummaryStartsInRequested();
        LenientRetroPaySummaryMovesToGranted();
        LenientRetroPaySummaryRejectsMovesFromExpired();
    }
}
