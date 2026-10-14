namespace Acme.Workforce.Absence.Tests.Balance.Quota;

using Acme.Workforce.Absence.Balance.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RollingBalanceQuotaEngineChecks
{
    public static void RollingBalanceQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = RollingBalanceQuotaEngine.DefaultRollingBalanceQuotaEngine(Flags.NewRegistry());
        var item = RollingBalanceQuota.NewRollingBalanceQuota("code-773", "title-724");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingBalanceQuotaEngineKeepsResultsWithinBounds();
    }
}
