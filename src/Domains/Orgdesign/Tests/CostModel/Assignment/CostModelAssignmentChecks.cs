namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelAssignmentChecks
{
    public static void CostModelAssignmentStartsInIdle()
    {
        var item = CostModelAssignment.NewCostModelAssignment("code-948", "label-57");
        Check.Equal(item.Status, CostModelAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelAssignmentMovesToStopped()
    {
        var item = CostModelAssignment.NewCostModelAssignment("code-948", "label-57");
        try
        {
            item.MoveTo(CostModelAssignmentStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelAssignmentStatus.Stopped, "status");
    }

    public static void CostModelAssignmentRejectsMovesFromStopped()
    {
        var item = CostModelAssignment.NewCostModelAssignment("code-948", "label-57");
        item.Status = CostModelAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(CostModelAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CostModelAssignmentStartsInIdle();
        CostModelAssignmentMovesToStopped();
        CostModelAssignmentRejectsMovesFromStopped();
    }
}
