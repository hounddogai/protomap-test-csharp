namespace Acme.Workforce.Compensation.Tests.EquityGrant.Quota;

using Acme.Workforce.Compensation.EquityGrant.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScheduledEquityGrantQuotaEngineChecks
{
    public static void ScheduledEquityGrantQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledEquityGrantQuotaEngine.DefaultScheduledEquityGrantQuotaEngine(Flags.NewRegistry());
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("code-60", "title-629");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantQuotaEngineKeepsResultsWithinBounds();
    }
}
