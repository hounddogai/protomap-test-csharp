namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelAssignmentEngineChecks
{
    public static void CostModelAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelAssignmentEngine.DefaultCostModelAssignmentEngine(Flags.NewRegistry());
        var item = CostModelAssignment.NewCostModelAssignment("code-948", "label-57");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelAssignmentEngineKeepsResultsWithinBounds();
    }
}
