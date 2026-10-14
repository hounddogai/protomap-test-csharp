namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Quota;

using Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RollingSuccessionPlanRequestChecks
{
    public static void RollingSuccessionPlanRequestStartsInProposed()
    {
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("code-368", "name-342");
        Check.Equal(item.Status, RollingSuccessionPlanRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSuccessionPlanRequestMovesToRejected()
    {
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("code-368", "name-342");
        try
        {
            item.MoveTo(RollingSuccessionPlanRequestStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSuccessionPlanRequestStatus.Rejected, "status");
    }

    public static void RollingSuccessionPlanRequestRejectsMovesFromWithdrawn()
    {
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("code-368", "name-342");
        item.Status = RollingSuccessionPlanRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingSuccessionPlanRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanRequestStartsInProposed();
        RollingSuccessionPlanRequestMovesToRejected();
        RollingSuccessionPlanRequestRejectsMovesFromWithdrawn();
    }
}
