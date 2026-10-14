namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackLicenseSeatBudgetChecks
{
    public static void FallbackLicenseSeatBudgetStartsInRequested()
    {
        var item = FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget("code-877", "title-400");
        Check.Equal(item.Status, FallbackLicenseSeatBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackLicenseSeatBudgetMovesToGranted()
    {
        var item = FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget("code-877", "title-400");
        try
        {
            item.MoveTo(FallbackLicenseSeatBudgetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackLicenseSeatBudgetStatus.Granted, "status");
    }

    public static void FallbackLicenseSeatBudgetRejectsMovesFromExpired()
    {
        var item = FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget("code-877", "title-400");
        item.Status = FallbackLicenseSeatBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackLicenseSeatBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatBudgetStartsInRequested();
        FallbackLicenseSeatBudgetMovesToGranted();
        FallbackLicenseSeatBudgetRejectsMovesFromExpired();
    }
}
