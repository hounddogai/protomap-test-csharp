namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelAssignmentEngineChecks
{
    public static void TieredCostModelAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = TieredCostModelAssignmentEngine.DefaultTieredCostModelAssignmentEngine(Flags.NewRegistry());
        var item = TieredCostModelAssignment.NewTieredCostModelAssignment("code-590", "name-22");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredCostModelAssignmentEngineKeepsResultsWithinBounds();
    }
}
