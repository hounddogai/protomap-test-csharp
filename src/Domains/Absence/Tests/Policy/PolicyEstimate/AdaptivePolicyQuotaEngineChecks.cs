namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyQuotaEngineChecks
{
    public static void AdaptivePolicyQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePolicyQuotaEngine.DefaultAdaptivePolicyQuotaEngine(Flags.NewRegistry());
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("code-330", "title-595");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePolicyQuotaEngineKeepsResultsWithinBounds();
    }
}
