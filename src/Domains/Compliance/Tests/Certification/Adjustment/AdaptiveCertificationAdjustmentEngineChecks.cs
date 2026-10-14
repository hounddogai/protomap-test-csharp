namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AdaptiveCertificationAdjustmentEngineChecks
{
    public static void AdaptiveCertificationAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveCertificationAdjustmentEngine.DefaultAdaptiveCertificationAdjustmentEngine(Flags.NewRegistry());
        var item = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment("code-79", "name-158");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveCertificationAdjustmentEngineKeepsResultsWithinBounds();
    }
}
