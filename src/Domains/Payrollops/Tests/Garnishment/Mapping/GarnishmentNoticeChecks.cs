namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GarnishmentNoticeChecks
{
    public static void GarnishmentNoticeStartsInDraft()
    {
        var item = GarnishmentNotice.NewGarnishmentNotice("code-192", "name-533");
        Check.Equal(item.Status, GarnishmentNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentNoticeMovesToSuspended()
    {
        var item = GarnishmentNotice.NewGarnishmentNotice("code-192", "name-533");
        try
        {
            item.MoveTo(GarnishmentNoticeStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentNoticeStatus.Suspended, "status");
    }

    public static void GarnishmentNoticeRejectsMovesFromClosed()
    {
        var item = GarnishmentNotice.NewGarnishmentNotice("code-192", "name-533");
        item.Status = GarnishmentNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(GarnishmentNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GarnishmentNoticeStartsInDraft();
        GarnishmentNoticeMovesToSuspended();
        GarnishmentNoticeRejectsMovesFromClosed();
    }
}
