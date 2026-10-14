namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CareerPathBudgetChecks
{
    public static void CareerPathBudgetStartsInIdle()
    {
        var item = CareerPathBudget.NewCareerPathBudget("code-906", "name-726");
        Check.Equal(item.Status, CareerPathBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathBudgetMovesToWarming()
    {
        var item = CareerPathBudget.NewCareerPathBudget("code-906", "name-726");
        try
        {
            item.MoveTo(CareerPathBudgetStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathBudgetStatus.Warming, "status");
    }

    public static void CareerPathBudgetRejectsMovesFromStopped()
    {
        var item = CareerPathBudget.NewCareerPathBudget("code-906", "name-726");
        item.Status = CareerPathBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(CareerPathBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CareerPathBudgetStartsInIdle();
        CareerPathBudgetMovesToWarming();
        CareerPathBudgetRejectsMovesFromStopped();
    }
}
