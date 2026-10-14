namespace Acme.Workforce.Compliance.Tests.Audit.Adjustment;

using Acme.Workforce.Compliance.Audit.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AuditAdjustmentEngineChecks
{
    public static void AuditAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = AuditAdjustmentEngine.DefaultAuditAdjustmentEngine(Flags.NewRegistry());
        var item = AuditAdjustment.NewAuditAdjustment("code-109", "title-26");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditAdjustmentEngineKeepsResultsWithinBounds();
    }
}
