namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class GlobalCostModelQuotaEngineChecks
{
    public static void GlobalCostModelQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalCostModelQuotaEngine.DefaultGlobalCostModelQuotaEngine(Flags.NewRegistry());
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("code-726", "name-452");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalCostModelQuotaEngineKeepsResultsWithinBounds();
    }
}
