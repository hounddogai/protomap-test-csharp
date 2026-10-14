namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ReorgAssignmentChecks
{
    public static void ReorgAssignmentStartsInRequested()
    {
        var item = ReorgAssignment.NewReorgAssignment("code-814", "title-183");
        Check.Equal(item.Status, ReorgAssignmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgAssignmentMovesToRevoked()
    {
        var item = ReorgAssignment.NewReorgAssignment("code-814", "title-183");
        try
        {
            item.MoveTo(ReorgAssignmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgAssignmentStatus.Revoked, "status");
    }

    public static void ReorgAssignmentRejectsMovesFromExpired()
    {
        var item = ReorgAssignment.NewReorgAssignment("code-814", "title-183");
        item.Status = ReorgAssignmentStatus.Expired;
        Check.Fails(() => item.MoveTo(ReorgAssignmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ReorgAssignmentStartsInRequested();
        ReorgAssignmentMovesToRevoked();
        ReorgAssignmentRejectsMovesFromExpired();
    }
}
