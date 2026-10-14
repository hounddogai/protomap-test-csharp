namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateNoticeChecks
{
    public static void RateNoticeStartsInCreated()
    {
        var item = RateNotice.NewRateNotice("code-383", "label-716");
        Check.Equal(item.Status, RateNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateNoticeMovesToValidated()
    {
        var item = RateNotice.NewRateNotice("code-383", "label-716");
        try
        {
            item.MoveTo(RateNoticeStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateNoticeStatus.Validated, "status");
    }

    public static void RateNoticeRejectsMovesFromRetired()
    {
        var item = RateNotice.NewRateNotice("code-383", "label-716");
        item.Status = RateNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(RateNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        RateNoticeStartsInCreated();
        RateNoticeMovesToValidated();
        RateNoticeRejectsMovesFromRetired();
    }
}
