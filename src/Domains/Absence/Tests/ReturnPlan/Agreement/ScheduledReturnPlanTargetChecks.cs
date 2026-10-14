namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ScheduledReturnPlanTargetChecks
{
    public static void ScheduledReturnPlanTargetStartsInCreated()
    {
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("code-421", "label-18");
        Check.Equal(item.Status, ScheduledReturnPlanTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledReturnPlanTargetMovesToValidated()
    {
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("code-421", "label-18");
        try
        {
            item.MoveTo(ScheduledReturnPlanTargetStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledReturnPlanTargetStatus.Validated, "status");
    }

    public static void ScheduledReturnPlanTargetRejectsMovesFromRetired()
    {
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("code-421", "label-18");
        item.Status = ScheduledReturnPlanTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledReturnPlanTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledReturnPlanTargetStartsInCreated();
        ScheduledReturnPlanTargetMovesToValidated();
        ScheduledReturnPlanTargetRejectsMovesFromRetired();
    }
}
