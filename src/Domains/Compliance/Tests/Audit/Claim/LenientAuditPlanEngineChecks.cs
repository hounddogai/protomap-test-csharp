namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientAuditPlanEngineChecks
{
    public static void LenientAuditPlanEngineKeepsResultsWithinBounds()
    {
        var engine = LenientAuditPlanEngine.DefaultLenientAuditPlanEngine();
        var item = LenientAuditPlan.NewLenientAuditPlan("code-474", "label-825");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientAuditPlanEngineKeepsResultsWithinBounds();
    }
}
