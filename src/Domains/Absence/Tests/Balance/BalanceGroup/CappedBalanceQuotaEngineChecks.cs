namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class CappedBalanceQuotaEngineChecks
{
    public static void CappedBalanceQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = CappedBalanceQuotaEngine.DefaultCappedBalanceQuotaEngine(Flags.NewRegistry());
        var item = CappedBalanceQuota.NewCappedBalanceQuota("code-754", "label-896");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedBalanceQuotaEngineKeepsResultsWithinBounds();
    }
}
