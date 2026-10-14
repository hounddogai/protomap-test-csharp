namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalExceptionCheckpointChecks
{
    public static void RegionalExceptionCheckpointStartsInProposed()
    {
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("code-91", "title-487");
        Check.Equal(item.Status, RegionalExceptionCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalExceptionCheckpointMovesToApproved()
    {
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("code-91", "title-487");
        try
        {
            item.MoveTo(RegionalExceptionCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalExceptionCheckpointStatus.Approved, "status");
    }

    public static void RegionalExceptionCheckpointRejectsMovesFromWithdrawn()
    {
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("code-91", "title-487");
        item.Status = RegionalExceptionCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalExceptionCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalExceptionCheckpointStartsInProposed();
        RegionalExceptionCheckpointMovesToApproved();
        RegionalExceptionCheckpointRejectsMovesFromWithdrawn();
    }
}
