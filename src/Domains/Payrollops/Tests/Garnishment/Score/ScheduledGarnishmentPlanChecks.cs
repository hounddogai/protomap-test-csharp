namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledGarnishmentPlanChecks
{
    public static void ScheduledGarnishmentPlanStartsInProposed()
    {
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("code-702", "title-667");
        Check.Equal(item.Status, ScheduledGarnishmentPlanStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledGarnishmentPlanMovesToRejected()
    {
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("code-702", "title-667");
        try
        {
            item.MoveTo(ScheduledGarnishmentPlanStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledGarnishmentPlanStatus.Rejected, "status");
    }

    public static void ScheduledGarnishmentPlanRejectsMovesFromWithdrawn()
    {
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("code-702", "title-667");
        item.Status = ScheduledGarnishmentPlanStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledGarnishmentPlanStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledGarnishmentPlanStartsInProposed();
        ScheduledGarnishmentPlanMovesToRejected();
        ScheduledGarnishmentPlanRejectsMovesFromWithdrawn();
    }
}
