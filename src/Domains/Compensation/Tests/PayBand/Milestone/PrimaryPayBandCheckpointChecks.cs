namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PrimaryPayBandCheckpointChecks
{
    public static void PrimaryPayBandCheckpointStartsInNew()
    {
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("code-226", "label-380");
        Check.Equal(item.Status, PrimaryPayBandCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPayBandCheckpointMovesToQueued()
    {
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("code-226", "label-380");
        try
        {
            item.MoveTo(PrimaryPayBandCheckpointStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPayBandCheckpointStatus.Queued, "status");
    }

    public static void PrimaryPayBandCheckpointRejectsMovesFromFailed()
    {
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("code-226", "label-380");
        item.Status = PrimaryPayBandCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(PrimaryPayBandCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        PrimaryPayBandCheckpointStartsInNew();
        PrimaryPayBandCheckpointMovesToQueued();
        PrimaryPayBandCheckpointRejectsMovesFromFailed();
    }
}
