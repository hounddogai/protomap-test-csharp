namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionGroupEngineChecks
{
    public static void DeductionGroupEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionGroupEngine.DefaultDeductionGroupEngine(Flags.NewRegistry());
        var item = DeductionGroup.NewDeductionGroup("code-742", "title-302");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionGroupEngineKeepsResultsWithinBounds();
    }
}
