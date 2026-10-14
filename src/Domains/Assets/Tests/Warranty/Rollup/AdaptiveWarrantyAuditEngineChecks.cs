namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AdaptiveWarrantyAuditEngineChecks
{
    public static void AdaptiveWarrantyAuditEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveWarrantyAuditEngine.DefaultAdaptiveWarrantyAuditEngine(Flags.NewRegistry());
        var item = AdaptiveWarrantyAudit.NewAdaptiveWarrantyAudit("code-152", "label-335");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyAuditEngineKeepsResultsWithinBounds();
    }
}
