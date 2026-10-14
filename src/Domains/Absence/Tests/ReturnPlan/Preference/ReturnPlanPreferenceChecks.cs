namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanPreferenceChecks
{
    public static void ReturnPlanPreferenceStartsInIdle()
    {
        var item = ReturnPlanPreference.NewReturnPlanPreference("code-652", "title-590");
        Check.Equal(item.Status, ReturnPlanPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanPreferenceMovesToStopped()
    {
        var item = ReturnPlanPreference.NewReturnPlanPreference("code-652", "title-590");
        try
        {
            item.MoveTo(ReturnPlanPreferenceStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanPreferenceStatus.Stopped, "status");
    }

    public static void ReturnPlanPreferenceRejectsMovesFromStopped()
    {
        var item = ReturnPlanPreference.NewReturnPlanPreference("code-652", "title-590");
        item.Status = ReturnPlanPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReturnPlanPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReturnPlanPreferenceStartsInIdle();
        ReturnPlanPreferenceMovesToStopped();
        ReturnPlanPreferenceRejectsMovesFromStopped();
    }
}
