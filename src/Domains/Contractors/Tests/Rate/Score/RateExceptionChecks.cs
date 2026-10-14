namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RateExceptionChecks
{
    public static void RateExceptionStartsInRequested()
    {
        var item = RateException.NewRateException("code-18", "title-77");
        Check.Equal(item.Status, RateExceptionStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateExceptionMovesToExpired()
    {
        var item = RateException.NewRateException("code-18", "title-77");
        try
        {
            item.MoveTo(RateExceptionStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateExceptionStatus.Expired, "status");
    }

    public static void RateExceptionRejectsMovesFromExpired()
    {
        var item = RateException.NewRateException("code-18", "title-77");
        item.Status = RateExceptionStatus.Expired;
        Check.Fails(() => item.MoveTo(RateExceptionStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RateExceptionStartsInRequested();
        RateExceptionMovesToExpired();
        RateExceptionRejectsMovesFromExpired();
    }
}
