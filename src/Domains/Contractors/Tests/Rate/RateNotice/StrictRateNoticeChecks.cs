namespace Acme.Workforce.Contractors.Tests.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.RateNotice;

/// <summary>Checks of rate notice code, run by the project's runner.</summary>
public static class StrictRateNoticeChecks
{
    public static void StrictRateNoticeStartsInNew()
    {
        var item = StrictRateNotice.NewStrictRateNotice("code-796", "name-953");
        Check.Equal(item.Status, StrictRateNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictRateNoticeMovesToRunning()
    {
        var item = StrictRateNotice.NewStrictRateNotice("code-796", "name-953");
        try
        {
            item.MoveTo(StrictRateNoticeStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictRateNoticeStatus.Running, "status");
    }

    public static void StrictRateNoticeRejectsMovesFromFailed()
    {
        var item = StrictRateNotice.NewStrictRateNotice("code-796", "name-953");
        item.Status = StrictRateNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictRateNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictRateNoticeStartsInNew();
        StrictRateNoticeMovesToRunning();
        StrictRateNoticeRejectsMovesFromFailed();
    }
}
