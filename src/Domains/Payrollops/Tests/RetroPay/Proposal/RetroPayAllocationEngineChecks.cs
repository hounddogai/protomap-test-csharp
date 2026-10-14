namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayAllocationEngineChecks
{
    public static void RetroPayAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayAllocationEngine.DefaultRetroPayAllocationEngine();
        var item = RetroPayAllocation.NewRetroPayAllocation("code-399", "label-484");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayAllocationEngineKeepsResultsWithinBounds();
    }
}
