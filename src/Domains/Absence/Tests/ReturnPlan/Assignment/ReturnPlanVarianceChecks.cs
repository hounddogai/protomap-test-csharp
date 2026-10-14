namespace Acme.Workforce.Absence.Tests.ReturnPlan.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ReturnPlanVarianceChecks
{
    public static void ReturnPlanVarianceStartsInRequested()
    {
        var item = ReturnPlanVariance.NewReturnPlanVariance("code-977", "name-35");
        Check.Equal(item.Status, ReturnPlanVarianceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanVarianceMovesToGranted()
    {
        var item = ReturnPlanVariance.NewReturnPlanVariance("code-977", "name-35");
        try
        {
            item.MoveTo(ReturnPlanVarianceStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanVarianceStatus.Granted, "status");
    }

    public static void ReturnPlanVarianceRejectsMovesFromExpired()
    {
        var item = ReturnPlanVariance.NewReturnPlanVariance("code-977", "name-35");
        item.Status = ReturnPlanVarianceStatus.Expired;
        Check.Fails(() => item.MoveTo(ReturnPlanVarianceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ReturnPlanVarianceStartsInRequested();
        ReturnPlanVarianceMovesToGranted();
        ReturnPlanVarianceRejectsMovesFromExpired();
    }
}
