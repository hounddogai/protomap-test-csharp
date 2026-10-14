namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanProfileChecks
{
    public static void ReturnPlanProfileStartsInIdle()
    {
        var item = ReturnPlanProfile.NewReturnPlanProfile("code-863", "name-483");
        Check.Equal(item.Status, ReturnPlanProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanProfileMovesToStopped()
    {
        var item = ReturnPlanProfile.NewReturnPlanProfile("code-863", "name-483");
        try
        {
            item.MoveTo(ReturnPlanProfileStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanProfileStatus.Stopped, "status");
    }

    public static void ReturnPlanProfileRejectsMovesFromStopped()
    {
        var item = ReturnPlanProfile.NewReturnPlanProfile("code-863", "name-483");
        item.Status = ReturnPlanProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReturnPlanProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReturnPlanProfileStartsInIdle();
        ReturnPlanProfileMovesToStopped();
        ReturnPlanProfileRejectsMovesFromStopped();
    }
}
