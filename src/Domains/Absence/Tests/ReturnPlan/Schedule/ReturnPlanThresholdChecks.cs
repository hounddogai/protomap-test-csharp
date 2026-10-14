namespace Acme.Workforce.Absence.Tests.ReturnPlan.Schedule;

using Acme.Workforce.Absence.ReturnPlan.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReturnPlanThresholdChecks
{
    public static void ReturnPlanThresholdStartsInRequested()
    {
        var item = ReturnPlanThreshold.NewReturnPlanThreshold("code-988", "title-497");
        Check.Equal(item.Status, ReturnPlanThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanThresholdMovesToExpired()
    {
        var item = ReturnPlanThreshold.NewReturnPlanThreshold("code-988", "title-497");
        try
        {
            item.MoveTo(ReturnPlanThresholdStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanThresholdStatus.Expired, "status");
    }

    public static void ReturnPlanThresholdRejectsMovesFromExpired()
    {
        var item = ReturnPlanThreshold.NewReturnPlanThreshold("code-988", "title-497");
        item.Status = ReturnPlanThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(ReturnPlanThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ReturnPlanThresholdStartsInRequested();
        ReturnPlanThresholdMovesToExpired();
        ReturnPlanThresholdRejectsMovesFromExpired();
    }
}
