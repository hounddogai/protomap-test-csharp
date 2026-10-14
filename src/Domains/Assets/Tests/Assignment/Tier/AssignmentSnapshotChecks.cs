namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AssignmentSnapshotChecks
{
    public static void AssignmentSnapshotStartsInCreated()
    {
        var item = AssignmentSnapshot.NewAssignmentSnapshot("code-559", "name-697");
        Check.Equal(item.Status, AssignmentSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentSnapshotMovesToValidated()
    {
        var item = AssignmentSnapshot.NewAssignmentSnapshot("code-559", "name-697");
        try
        {
            item.MoveTo(AssignmentSnapshotStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentSnapshotStatus.Validated, "status");
    }

    public static void AssignmentSnapshotRejectsMovesFromRetired()
    {
        var item = AssignmentSnapshot.NewAssignmentSnapshot("code-559", "name-697");
        item.Status = AssignmentSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(AssignmentSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssignmentSnapshotStartsInCreated();
        AssignmentSnapshotMovesToValidated();
        AssignmentSnapshotRejectsMovesFromRetired();
    }
}
