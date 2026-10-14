namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalancePreferenceChecks
{
    public static void BalancePreferenceStartsInOpen()
    {
        var item = BalancePreference.NewBalancePreference("code-393", "title-884");
        Check.Equal(item.Status, BalancePreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalancePreferenceMovesToResolved()
    {
        var item = BalancePreference.NewBalancePreference("code-393", "title-884");
        try
        {
            item.MoveTo(BalancePreferenceStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalancePreferenceStatus.Resolved, "status");
    }

    public static void BalancePreferenceRejectsMovesFromArchived()
    {
        var item = BalancePreference.NewBalancePreference("code-393", "title-884");
        item.Status = BalancePreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(BalancePreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        BalancePreferenceStartsInOpen();
        BalancePreferenceMovesToResolved();
        BalancePreferenceRejectsMovesFromArchived();
    }
}
