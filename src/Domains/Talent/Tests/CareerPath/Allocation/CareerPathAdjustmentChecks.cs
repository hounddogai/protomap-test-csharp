namespace Acme.Workforce.Talent.Tests.CareerPath.Allocation;

using Acme.Workforce.Talent.CareerPath.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class CareerPathAdjustmentChecks
{
    public static void CareerPathAdjustmentStartsInOpen()
    {
        var item = CareerPathAdjustment.NewCareerPathAdjustment("code-447", "title-343");
        Check.Equal(item.Status, CareerPathAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathAdjustmentMovesToReopened()
    {
        var item = CareerPathAdjustment.NewCareerPathAdjustment("code-447", "title-343");
        try
        {
            item.MoveTo(CareerPathAdjustmentStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathAdjustmentStatus.Reopened, "status");
    }

    public static void CareerPathAdjustmentRejectsMovesFromArchived()
    {
        var item = CareerPathAdjustment.NewCareerPathAdjustment("code-447", "title-343");
        item.Status = CareerPathAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(CareerPathAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        CareerPathAdjustmentStartsInOpen();
        CareerPathAdjustmentMovesToReopened();
        CareerPathAdjustmentRejectsMovesFromArchived();
    }
}
