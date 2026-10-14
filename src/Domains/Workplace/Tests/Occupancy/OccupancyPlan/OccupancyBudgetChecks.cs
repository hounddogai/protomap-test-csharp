namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyBudgetChecks
{
    public static void OccupancyBudgetStartsInCreated()
    {
        var item = OccupancyBudget.NewOccupancyBudget("code-237", "label-556");
        Check.Equal(item.Status, OccupancyBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyBudgetMovesToPublished()
    {
        var item = OccupancyBudget.NewOccupancyBudget("code-237", "label-556");
        try
        {
            item.MoveTo(OccupancyBudgetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyBudgetStatus.Published, "status");
    }

    public static void OccupancyBudgetRejectsMovesFromRetired()
    {
        var item = OccupancyBudget.NewOccupancyBudget("code-237", "label-556");
        item.Status = OccupancyBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(OccupancyBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        OccupancyBudgetStartsInCreated();
        OccupancyBudgetMovesToPublished();
        OccupancyBudgetRejectsMovesFromRetired();
    }
}
