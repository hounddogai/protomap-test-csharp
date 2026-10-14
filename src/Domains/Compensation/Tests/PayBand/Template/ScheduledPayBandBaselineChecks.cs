namespace Acme.Workforce.Compensation.Tests.PayBand.Template;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledPayBandBaselineChecks
{
    public static void ScheduledPayBandBaselineStartsInDraft()
    {
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("code-669", "title-544");
        Check.Equal(item.Status, ScheduledPayBandBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayBandBaselineMovesToSuspended()
    {
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("code-669", "title-544");
        try
        {
            item.MoveTo(ScheduledPayBandBaselineStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayBandBaselineStatus.Suspended, "status");
    }

    public static void ScheduledPayBandBaselineRejectsMovesFromClosed()
    {
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("code-669", "title-544");
        item.Status = ScheduledPayBandBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledPayBandBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledPayBandBaselineStartsInDraft();
        ScheduledPayBandBaselineMovesToSuspended();
        ScheduledPayBandBaselineRejectsMovesFromClosed();
    }
}
