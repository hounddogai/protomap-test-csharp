namespace Acme.Workforce.Payrollops.Tests.Deduction.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DeductionCycleEngineChecks
{
    public static void DeductionCycleEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionCycleEngine.DefaultDeductionCycleEngine(Flags.NewRegistry());
        var item = DeductionCycle.NewDeductionCycle("code-764", "name-510");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionCycleEngineKeepsResultsWithinBounds();
    }
}
