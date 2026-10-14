namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class LegacyHeadcountBudgetChecks
{
    public static void LegacyHeadcountBudgetStartsInRequested()
    {
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("code-63", "title-769");
        Check.Equal(item.Status, LegacyHeadcountBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyHeadcountBudgetMovesToGranted()
    {
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("code-63", "title-769");
        try
        {
            item.MoveTo(LegacyHeadcountBudgetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyHeadcountBudgetStatus.Granted, "status");
    }

    public static void LegacyHeadcountBudgetRejectsMovesFromExpired()
    {
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("code-63", "title-769");
        item.Status = LegacyHeadcountBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(LegacyHeadcountBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LegacyHeadcountBudgetStartsInRequested();
        LegacyHeadcountBudgetMovesToGranted();
        LegacyHeadcountBudgetRejectsMovesFromExpired();
    }
}
