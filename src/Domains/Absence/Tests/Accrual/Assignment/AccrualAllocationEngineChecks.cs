namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAllocationEngineChecks
{
    public static void AccrualAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualAllocationEngine.DefaultAccrualAllocationEngine();
        var item = AccrualAllocation.NewAccrualAllocation("code-479", "name-399");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualAllocationEngineKeepsResultsWithinBounds();
    }
}
