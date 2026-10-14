namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class StrictAssignmentCheckpointChecks
{
    public static void StrictAssignmentCheckpointStartsInCreated()
    {
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("code-8", "label-526");
        Check.Equal(item.Status, StrictAssignmentCheckpointStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAssignmentCheckpointMovesToValidated()
    {
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("code-8", "label-526");
        try
        {
            item.MoveTo(StrictAssignmentCheckpointStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAssignmentCheckpointStatus.Validated, "status");
    }

    public static void StrictAssignmentCheckpointRejectsMovesFromRetired()
    {
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("code-8", "label-526");
        item.Status = StrictAssignmentCheckpointStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictAssignmentCheckpointStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictAssignmentCheckpointStartsInCreated();
        StrictAssignmentCheckpointMovesToValidated();
        StrictAssignmentCheckpointRejectsMovesFromRetired();
    }
}
