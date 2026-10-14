namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityCheckpointChecks
{
    public static void MobilityCheckpointStartsInOpen()
    {
        var item = MobilityCheckpoint.NewMobilityCheckpoint("code-150", "name-773");
        Check.Equal(item.Status, MobilityCheckpointStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityCheckpointMovesToInReview()
    {
        var item = MobilityCheckpoint.NewMobilityCheckpoint("code-150", "name-773");
        try
        {
            item.MoveTo(MobilityCheckpointStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityCheckpointStatus.InReview, "status");
    }

    public static void MobilityCheckpointRejectsMovesFromArchived()
    {
        var item = MobilityCheckpoint.NewMobilityCheckpoint("code-150", "name-773");
        item.Status = MobilityCheckpointStatus.Archived;
        Check.Fails(() => item.MoveTo(MobilityCheckpointStatus.Open), "move");
    }

    public static void RunAll()
    {
        MobilityCheckpointStartsInOpen();
        MobilityCheckpointMovesToInReview();
        MobilityCheckpointRejectsMovesFromArchived();
    }
}
