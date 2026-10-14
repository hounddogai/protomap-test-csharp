namespace Acme.Workforce.Contractors.Tests.Rate.RateReview;

using Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>Checks of rate review code, run by the project's runner.</summary>
public static class RollingRateAssignmentChecks
{
    public static void RollingRateAssignmentStartsInRequested()
    {
        var item = RollingRateAssignment.NewRollingRateAssignment("code-204", "label-291");
        Check.Equal(item.Status, RollingRateAssignmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRateAssignmentMovesToRevoked()
    {
        var item = RollingRateAssignment.NewRollingRateAssignment("code-204", "label-291");
        try
        {
            item.MoveTo(RollingRateAssignmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRateAssignmentStatus.Revoked, "status");
    }

    public static void RollingRateAssignmentRejectsMovesFromExpired()
    {
        var item = RollingRateAssignment.NewRollingRateAssignment("code-204", "label-291");
        item.Status = RollingRateAssignmentStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingRateAssignmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingRateAssignmentStartsInRequested();
        RollingRateAssignmentMovesToRevoked();
        RollingRateAssignmentRejectsMovesFromExpired();
    }
}
