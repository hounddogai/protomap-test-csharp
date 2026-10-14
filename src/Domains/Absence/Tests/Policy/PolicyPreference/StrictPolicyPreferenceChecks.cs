namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class StrictPolicyPreferenceChecks
{
    public static void StrictPolicyPreferenceStartsInIdle()
    {
        var item = StrictPolicyPreference.NewStrictPolicyPreference("code-691", "title-468");
        Check.Equal(item.Status, StrictPolicyPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPolicyPreferenceMovesToWarming()
    {
        var item = StrictPolicyPreference.NewStrictPolicyPreference("code-691", "title-468");
        try
        {
            item.MoveTo(StrictPolicyPreferenceStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPolicyPreferenceStatus.Warming, "status");
    }

    public static void StrictPolicyPreferenceRejectsMovesFromStopped()
    {
        var item = StrictPolicyPreference.NewStrictPolicyPreference("code-691", "title-468");
        item.Status = StrictPolicyPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictPolicyPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictPolicyPreferenceStartsInIdle();
        StrictPolicyPreferenceMovesToWarming();
        StrictPolicyPreferenceRejectsMovesFromStopped();
    }
}
