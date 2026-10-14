namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatBudgetChecks
{
    public static void LicenseSeatBudgetStartsInNew()
    {
        var item = LicenseSeatBudget.NewLicenseSeatBudget("code-441", "label-578");
        Check.Equal(item.Status, LicenseSeatBudgetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatBudgetMovesToDone()
    {
        var item = LicenseSeatBudget.NewLicenseSeatBudget("code-441", "label-578");
        try
        {
            item.MoveTo(LicenseSeatBudgetStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatBudgetStatus.Done, "status");
    }

    public static void LicenseSeatBudgetRejectsMovesFromFailed()
    {
        var item = LicenseSeatBudget.NewLicenseSeatBudget("code-441", "label-578");
        item.Status = LicenseSeatBudgetStatus.Failed;
        Check.Fails(() => item.MoveTo(LicenseSeatBudgetStatus.New), "move");
    }

    public static void RunAll()
    {
        LicenseSeatBudgetStartsInNew();
        LicenseSeatBudgetMovesToDone();
        LicenseSeatBudgetRejectsMovesFromFailed();
    }
}
