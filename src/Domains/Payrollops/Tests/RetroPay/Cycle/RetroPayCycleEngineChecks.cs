namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayCycleEngineChecks
{
    public static void RetroPayCycleEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayCycleEngine.DefaultRetroPayCycleEngine();
        var item = RetroPayCycle.NewRetroPayCycle("code-485", "name-145");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayCycleEngineKeepsResultsWithinBounds();
    }
}
