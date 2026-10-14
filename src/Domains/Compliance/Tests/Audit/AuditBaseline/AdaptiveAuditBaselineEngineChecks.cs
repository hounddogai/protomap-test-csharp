namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class AdaptiveAuditBaselineEngineChecks
{
    public static void AdaptiveAuditBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveAuditBaselineEngine.DefaultAdaptiveAuditBaselineEngine(Flags.NewRegistry());
        var item = AdaptiveAuditBaseline.NewAdaptiveAuditBaseline("code-639", "name-411");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveAuditBaselineEngineKeepsResultsWithinBounds();
    }
}
