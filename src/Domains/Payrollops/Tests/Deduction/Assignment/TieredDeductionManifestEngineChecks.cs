namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredDeductionManifestEngineChecks
{
    public static void TieredDeductionManifestEngineKeepsResultsWithinBounds()
    {
        var engine = TieredDeductionManifestEngine.DefaultTieredDeductionManifestEngine(Flags.NewRegistry());
        var item = TieredDeductionManifest.NewTieredDeductionManifest("code-614", "label-816");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredDeductionManifestEngineKeepsResultsWithinBounds();
    }
}
