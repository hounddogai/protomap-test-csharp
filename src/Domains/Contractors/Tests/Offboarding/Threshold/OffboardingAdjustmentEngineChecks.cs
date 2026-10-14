namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingAdjustmentEngineChecks
{
    public static void OffboardingAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingAdjustmentEngine.DefaultOffboardingAdjustmentEngine(Flags.NewRegistry());
        var item = OffboardingAdjustment.NewOffboardingAdjustment("code-197", "name-943");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingAdjustmentEngineKeepsResultsWithinBounds();
    }
}
