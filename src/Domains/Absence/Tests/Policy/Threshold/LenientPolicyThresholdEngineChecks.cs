namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientPolicyThresholdEngineChecks
{
    public static void LenientPolicyThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPolicyThresholdEngine.DefaultLenientPolicyThresholdEngine(Flags.NewRegistry());
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("code-693", "name-137");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPolicyThresholdEngineKeepsResultsWithinBounds();
    }
}
