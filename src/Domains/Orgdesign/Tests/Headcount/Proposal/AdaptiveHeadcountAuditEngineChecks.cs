namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AdaptiveHeadcountAuditEngineChecks
{
    public static void AdaptiveHeadcountAuditEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveHeadcountAuditEngine.DefaultAdaptiveHeadcountAuditEngine(Flags.NewRegistry());
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("code-192", "label-847");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountAuditEngineKeepsResultsWithinBounds();
    }
}
