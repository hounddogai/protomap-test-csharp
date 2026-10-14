namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultBalancePreferenceChecks
{
    public static void DefaultBalancePreferenceStartsInNew()
    {
        var item = DefaultBalancePreference.NewDefaultBalancePreference("code-733", "label-643");
        Check.Equal(item.Status, DefaultBalancePreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultBalancePreferenceMovesToRunning()
    {
        var item = DefaultBalancePreference.NewDefaultBalancePreference("code-733", "label-643");
        try
        {
            item.MoveTo(DefaultBalancePreferenceStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultBalancePreferenceStatus.Running, "status");
    }

    public static void DefaultBalancePreferenceRejectsMovesFromFailed()
    {
        var item = DefaultBalancePreference.NewDefaultBalancePreference("code-733", "label-643");
        item.Status = DefaultBalancePreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultBalancePreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultBalancePreferenceStartsInNew();
        DefaultBalancePreferenceMovesToRunning();
        DefaultBalancePreferenceRejectsMovesFromFailed();
    }
}
