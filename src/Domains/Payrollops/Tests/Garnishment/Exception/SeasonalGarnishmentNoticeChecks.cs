namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SeasonalGarnishmentNoticeChecks
{
    public static void SeasonalGarnishmentNoticeStartsInPlanned()
    {
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("code-855", "label-764");
        Check.Equal(item.Status, SeasonalGarnishmentNoticeStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalGarnishmentNoticeMovesToScheduled()
    {
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("code-855", "label-764");
        try
        {
            item.MoveTo(SeasonalGarnishmentNoticeStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalGarnishmentNoticeStatus.Scheduled, "status");
    }

    public static void SeasonalGarnishmentNoticeRejectsMovesFromCancelled()
    {
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("code-855", "label-764");
        item.Status = SeasonalGarnishmentNoticeStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalGarnishmentNoticeStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentNoticeStartsInPlanned();
        SeasonalGarnishmentNoticeMovesToScheduled();
        SeasonalGarnishmentNoticeRejectsMovesFromCancelled();
    }
}
