namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class RegionalCostModelAssignmentEngineChecks
{
    public static void RegionalCostModelAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalCostModelAssignmentEngine.DefaultRegionalCostModelAssignmentEngine(Flags.NewRegistry());
        var item = RegionalCostModelAssignment.NewRegionalCostModelAssignment("code-579", "name-471");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalCostModelAssignmentEngineKeepsResultsWithinBounds();
    }
}
