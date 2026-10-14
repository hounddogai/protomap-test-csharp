namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CareerPathCheckpointChecks
{
    public static void CareerPathCheckpointStartsInPlanned()
    {
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("code-395", "label-730");
        Check.Equal(item.Status, CareerPathCheckpointStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathCheckpointMovesToInProgress()
    {
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("code-395", "label-730");
        try
        {
            item.MoveTo(CareerPathCheckpointStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathCheckpointStatus.InProgress, "status");
    }

    public static void CareerPathCheckpointRejectsMovesFromCancelled()
    {
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("code-395", "label-730");
        item.Status = CareerPathCheckpointStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CareerPathCheckpointStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CareerPathCheckpointStartsInPlanned();
        CareerPathCheckpointMovesToInProgress();
        CareerPathCheckpointRejectsMovesFromCancelled();
    }
}
