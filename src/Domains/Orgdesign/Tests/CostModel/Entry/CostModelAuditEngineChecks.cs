namespace Acme.Workforce.Orgdesign.Tests.CostModel.Entry;

using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CostModelAuditEngineChecks
{
    public static void CostModelAuditEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelAuditEngine.DefaultCostModelAuditEngine();
        var item = CostModelAudit.NewCostModelAudit("code-297", "title-581");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelAuditEngineKeepsResultsWithinBounds();
    }
}
