namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RollingSuccessionPlanQuotaEngineChecks
{
    public static void RollingSuccessionPlanQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = RollingSuccessionPlanQuotaEngine.DefaultRollingSuccessionPlanQuotaEngine(Flags.NewRegistry());
        var item = RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota("code-174", "label-935");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanQuotaEngineKeepsResultsWithinBounds();
    }
}
