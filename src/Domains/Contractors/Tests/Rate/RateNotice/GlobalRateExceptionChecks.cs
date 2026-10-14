namespace Acme.Workforce.Contractors.Tests.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.RateNotice;

/// <summary>Checks of rate notice code, run by the project's runner.</summary>
public static class GlobalRateExceptionChecks
{
    public static void GlobalRateExceptionStartsInRequested()
    {
        var item = GlobalRateException.NewGlobalRateException("code-708", "label-551");
        Check.Equal(item.Status, GlobalRateExceptionStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalRateExceptionMovesToGranted()
    {
        var item = GlobalRateException.NewGlobalRateException("code-708", "label-551");
        try
        {
            item.MoveTo(GlobalRateExceptionStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalRateExceptionStatus.Granted, "status");
    }

    public static void GlobalRateExceptionRejectsMovesFromExpired()
    {
        var item = GlobalRateException.NewGlobalRateException("code-708", "label-551");
        item.Status = GlobalRateExceptionStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalRateExceptionStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalRateExceptionStartsInRequested();
        GlobalRateExceptionMovesToGranted();
        GlobalRateExceptionRejectsMovesFromExpired();
    }
}
