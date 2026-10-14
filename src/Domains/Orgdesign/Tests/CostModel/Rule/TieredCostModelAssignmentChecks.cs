namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelAssignmentChecks
{
    public static void TieredCostModelAssignmentStartsInCreated()
    {
        var item = TieredCostModelAssignment.NewTieredCostModelAssignment("code-590", "name-22");
        Check.Equal(item.Status, TieredCostModelAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredCostModelAssignmentMovesToValidated()
    {
        var item = TieredCostModelAssignment.NewTieredCostModelAssignment("code-590", "name-22");
        try
        {
            item.MoveTo(TieredCostModelAssignmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredCostModelAssignmentStatus.Validated, "status");
    }

    public static void TieredCostModelAssignmentRejectsMovesFromRetired()
    {
        var item = TieredCostModelAssignment.NewTieredCostModelAssignment("code-590", "name-22");
        item.Status = TieredCostModelAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredCostModelAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredCostModelAssignmentStartsInCreated();
        TieredCostModelAssignmentMovesToValidated();
        TieredCostModelAssignmentRejectsMovesFromRetired();
    }
}
