namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledAuditPlanChecks
{
    public static void ScheduledAuditPlanStartsInCreated()
    {
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("code-823", "name-11");
        Check.Equal(item.Status, ScheduledAuditPlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAuditPlanMovesToRetired()
    {
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("code-823", "name-11");
        try
        {
            item.MoveTo(ScheduledAuditPlanStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAuditPlanStatus.Retired, "status");
    }

    public static void ScheduledAuditPlanRejectsMovesFromRetired()
    {
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("code-823", "name-11");
        item.Status = ScheduledAuditPlanStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledAuditPlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledAuditPlanStartsInCreated();
        ScheduledAuditPlanMovesToRetired();
        ScheduledAuditPlanRejectsMovesFromRetired();
    }
}
