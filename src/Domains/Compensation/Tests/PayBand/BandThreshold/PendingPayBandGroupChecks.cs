namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PendingPayBandGroupChecks
{
    public static void PendingPayBandGroupStartsInDraft()
    {
        var item = PendingPayBandGroup.NewPendingPayBandGroup("code-335", "label-42");
        Check.Equal(item.Status, PendingPayBandGroupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPayBandGroupMovesToPending()
    {
        var item = PendingPayBandGroup.NewPendingPayBandGroup("code-335", "label-42");
        try
        {
            item.MoveTo(PendingPayBandGroupStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPayBandGroupStatus.Pending, "status");
    }

    public static void PendingPayBandGroupRejectsMovesFromClosed()
    {
        var item = PendingPayBandGroup.NewPendingPayBandGroup("code-335", "label-42");
        item.Status = PendingPayBandGroupStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingPayBandGroupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingPayBandGroupStartsInDraft();
        PendingPayBandGroupMovesToPending();
        PendingPayBandGroupRejectsMovesFromClosed();
    }
}
