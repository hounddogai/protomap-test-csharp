namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedAuditPlanEngineChecks
{
    public static void CappedAuditPlanEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAuditPlanEngine.DefaultCappedAuditPlanEngine(Flags.NewRegistry());
        var item = CappedAuditPlan.NewCappedAuditPlan("code-43", "label-39");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAuditPlanEngineKeepsResultsWithinBounds();
    }
}
