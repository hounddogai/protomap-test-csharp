namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedCostModelAssignmentEngineChecks
{
    public static void CappedCostModelAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = CappedCostModelAssignmentEngine.DefaultCappedCostModelAssignmentEngine();
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("code-780", "name-182");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedCostModelAssignmentEngineKeepsResultsWithinBounds();
    }
}
