namespace Acme.Workforce.Assets.Tests.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssignmentQuotaChecks
{
    public static void AssignmentQuotaStartsInIdle()
    {
        var item = AssignmentQuota.NewAssignmentQuota("code-466", "label-77");
        Check.Equal(item.Status, AssignmentQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentQuotaMovesToDraining()
    {
        var item = AssignmentQuota.NewAssignmentQuota("code-466", "label-77");
        try
        {
            item.MoveTo(AssignmentQuotaStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentQuotaStatus.Draining, "status");
    }

    public static void AssignmentQuotaRejectsMovesFromStopped()
    {
        var item = AssignmentQuota.NewAssignmentQuota("code-466", "label-77");
        item.Status = AssignmentQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(AssignmentQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AssignmentQuotaStartsInIdle();
        AssignmentQuotaMovesToDraining();
        AssignmentQuotaRejectsMovesFromStopped();
    }
}
