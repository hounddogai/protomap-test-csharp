namespace Acme.Workforce.Assets.Tests.Laptop.Draft;

using Acme.Workforce.Assets.Laptop.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LaptopBudgetChecks
{
    public static void LaptopBudgetStartsInRequested()
    {
        var item = LaptopBudget.NewLaptopBudget("code-434", "name-485");
        Check.Equal(item.Status, LaptopBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopBudgetMovesToRevoked()
    {
        var item = LaptopBudget.NewLaptopBudget("code-434", "name-485");
        try
        {
            item.MoveTo(LaptopBudgetStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopBudgetStatus.Revoked, "status");
    }

    public static void LaptopBudgetRejectsMovesFromExpired()
    {
        var item = LaptopBudget.NewLaptopBudget("code-434", "name-485");
        item.Status = LaptopBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(LaptopBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LaptopBudgetStartsInRequested();
        LaptopBudgetMovesToRevoked();
        LaptopBudgetRejectsMovesFromExpired();
    }
}
