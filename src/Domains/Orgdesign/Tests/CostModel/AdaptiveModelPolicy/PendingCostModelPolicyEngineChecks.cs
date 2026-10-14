namespace Acme.Workforce.Orgdesign.Tests.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Checks of adaptive model policy code, run by the project's runner.</summary>
public static class PendingCostModelPolicyEngineChecks
{
    public static void PendingCostModelPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = PendingCostModelPolicyEngine.DefaultPendingCostModelPolicyEngine(Flags.NewRegistry());
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("code-411", "label-378");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingCostModelPolicyEngineKeepsResultsWithinBounds();
    }
}
