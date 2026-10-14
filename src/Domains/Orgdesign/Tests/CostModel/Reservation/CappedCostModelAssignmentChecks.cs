namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedCostModelAssignmentChecks
{
    public static void CappedCostModelAssignmentStartsInCreated()
    {
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("code-780", "name-182");
        Check.Equal(item.Status, CappedCostModelAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCostModelAssignmentMovesToValidated()
    {
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("code-780", "name-182");
        try
        {
            item.MoveTo(CappedCostModelAssignmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCostModelAssignmentStatus.Validated, "status");
    }

    public static void CappedCostModelAssignmentRejectsMovesFromRetired()
    {
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("code-780", "name-182");
        item.Status = CappedCostModelAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedCostModelAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedCostModelAssignmentStartsInCreated();
        CappedCostModelAssignmentMovesToValidated();
        CappedCostModelAssignmentRejectsMovesFromRetired();
    }
}
