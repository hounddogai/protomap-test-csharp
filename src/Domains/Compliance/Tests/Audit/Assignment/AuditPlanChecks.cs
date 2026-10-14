namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditPlanChecks
{
    public static void AuditPlanStartsInPlanned()
    {
        var item = AuditPlan.NewAuditPlan("code-621", "name-121");
        Check.Equal(item.Status, AuditPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditPlanMovesToInProgress()
    {
        var item = AuditPlan.NewAuditPlan("code-621", "name-121");
        try
        {
            item.MoveTo(AuditPlanStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditPlanStatus.InProgress, "status");
    }

    public static void AuditPlanRejectsMovesFromCancelled()
    {
        var item = AuditPlan.NewAuditPlan("code-621", "name-121");
        item.Status = AuditPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AuditPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AuditPlanStartsInPlanned();
        AuditPlanMovesToInProgress();
        AuditPlanRejectsMovesFromCancelled();
    }
}
