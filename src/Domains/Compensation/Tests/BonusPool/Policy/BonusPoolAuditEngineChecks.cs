namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolAuditEngineChecks
{
    public static void BonusPoolAuditEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolAuditEngine.DefaultBonusPoolAuditEngine(Flags.NewRegistry());
        var item = BonusPoolAudit.NewBonusPoolAudit("code-651", "name-395");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolAuditEngineKeepsResultsWithinBounds();
    }
}
