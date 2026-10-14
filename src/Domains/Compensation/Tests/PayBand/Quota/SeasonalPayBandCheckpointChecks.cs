namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class SeasonalPayBandCheckpointChecks
{
    public static void SeasonalPayBandCheckpointStartsInDraft()
    {
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("code-962", "label-379");
        Check.Equal(item.Status, SeasonalPayBandCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPayBandCheckpointMovesToSuspended()
    {
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("code-962", "label-379");
        try
        {
            item.MoveTo(SeasonalPayBandCheckpointStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPayBandCheckpointStatus.Suspended, "status");
    }

    public static void SeasonalPayBandCheckpointRejectsMovesFromClosed()
    {
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("code-962", "label-379");
        item.Status = SeasonalPayBandCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalPayBandCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalPayBandCheckpointStartsInDraft();
        SeasonalPayBandCheckpointMovesToSuspended();
        SeasonalPayBandCheckpointRejectsMovesFromClosed();
    }
}
