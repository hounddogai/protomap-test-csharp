namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathCycleChecks
{
    public static void CareerPathCycleStartsInProposed()
    {
        var item = CareerPathCycle.NewCareerPathCycle("code-781", "name-312");
        Check.Equal(item.Status, CareerPathCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathCycleMovesToApproved()
    {
        var item = CareerPathCycle.NewCareerPathCycle("code-781", "name-312");
        try
        {
            item.MoveTo(CareerPathCycleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathCycleStatus.Approved, "status");
    }

    public static void CareerPathCycleRejectsMovesFromWithdrawn()
    {
        var item = CareerPathCycle.NewCareerPathCycle("code-781", "name-312");
        item.Status = CareerPathCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CareerPathCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CareerPathCycleStartsInProposed();
        CareerPathCycleMovesToApproved();
        CareerPathCycleRejectsMovesFromWithdrawn();
    }
}
