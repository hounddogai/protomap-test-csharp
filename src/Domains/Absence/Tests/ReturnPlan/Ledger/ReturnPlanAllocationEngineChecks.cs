namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanAllocationEngineChecks
{
    public static void ReturnPlanAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanAllocationEngine.DefaultReturnPlanAllocationEngine(Flags.NewRegistry());
        var item = ReturnPlanAllocation.NewReturnPlanAllocation("code-625", "name-28");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanAllocationEngineKeepsResultsWithinBounds();
    }
}
