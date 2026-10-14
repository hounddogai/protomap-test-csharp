namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyAllocationEngineChecks
{
    public static void AdaptivePolicyAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePolicyAllocationEngine.DefaultAdaptivePolicyAllocationEngine(Flags.NewRegistry());
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("code-759", "label-649");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePolicyAllocationEngineKeepsResultsWithinBounds();
    }
}
