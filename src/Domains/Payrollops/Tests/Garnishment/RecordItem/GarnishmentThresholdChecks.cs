namespace Acme.Workforce.Payrollops.Tests.Garnishment.RecordItem;

using Acme.Workforce.Payrollops.Garnishment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GarnishmentThresholdChecks
{
    public static void GarnishmentThresholdStartsInDraft()
    {
        var item = GarnishmentThreshold.NewGarnishmentThreshold("code-541", "label-29");
        Check.Equal(item.Status, GarnishmentThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentThresholdMovesToSuspended()
    {
        var item = GarnishmentThreshold.NewGarnishmentThreshold("code-541", "label-29");
        try
        {
            item.MoveTo(GarnishmentThresholdStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentThresholdStatus.Suspended, "status");
    }

    public static void GarnishmentThresholdRejectsMovesFromClosed()
    {
        var item = GarnishmentThreshold.NewGarnishmentThreshold("code-541", "label-29");
        item.Status = GarnishmentThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(GarnishmentThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GarnishmentThresholdStartsInDraft();
        GarnishmentThresholdMovesToSuspended();
        GarnishmentThresholdRejectsMovesFromClosed();
    }
}
