namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class RegionalCostModelAssignmentChecks
{
    public static void RegionalCostModelAssignmentStartsInDraft()
    {
        var item = RegionalCostModelAssignment.NewRegionalCostModelAssignment("code-579", "name-471");
        Check.Equal(item.Status, RegionalCostModelAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalCostModelAssignmentMovesToClosed()
    {
        var item = RegionalCostModelAssignment.NewRegionalCostModelAssignment("code-579", "name-471");
        try
        {
            item.MoveTo(RegionalCostModelAssignmentStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalCostModelAssignmentStatus.Closed, "status");
    }

    public static void RegionalCostModelAssignmentRejectsMovesFromClosed()
    {
        var item = RegionalCostModelAssignment.NewRegionalCostModelAssignment("code-579", "name-471");
        item.Status = RegionalCostModelAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalCostModelAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalCostModelAssignmentStartsInDraft();
        RegionalCostModelAssignmentMovesToClosed();
        RegionalCostModelAssignmentRejectsMovesFromClosed();
    }
}
