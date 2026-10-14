namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class ManualPolicyPreferenceChecks
{
    public static void ManualPolicyPreferenceStartsInNew()
    {
        var item = ManualPolicyPreference.NewManualPolicyPreference("code-3", "label-400");
        Check.Equal(item.Status, ManualPolicyPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPolicyPreferenceMovesToRunning()
    {
        var item = ManualPolicyPreference.NewManualPolicyPreference("code-3", "label-400");
        try
        {
            item.MoveTo(ManualPolicyPreferenceStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPolicyPreferenceStatus.Running, "status");
    }

    public static void ManualPolicyPreferenceRejectsMovesFromFailed()
    {
        var item = ManualPolicyPreference.NewManualPolicyPreference("code-3", "label-400");
        item.Status = ManualPolicyPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualPolicyPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualPolicyPreferenceStartsInNew();
        ManualPolicyPreferenceMovesToRunning();
        ManualPolicyPreferenceRejectsMovesFromFailed();
    }
}
