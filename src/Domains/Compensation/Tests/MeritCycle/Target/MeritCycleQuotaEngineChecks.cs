namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleQuotaEngineChecks
{
    public static void MeritCycleQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleQuotaEngine.DefaultMeritCycleQuotaEngine(Flags.NewRegistry());
        var item = MeritCycleQuota.NewMeritCycleQuota("code-296", "title-20");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleQuotaEngineKeepsResultsWithinBounds();
    }
}
