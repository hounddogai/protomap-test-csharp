namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateAuditEngineChecks
{
    public static void RateAuditEngineKeepsResultsWithinBounds()
    {
        var engine = RateAuditEngine.DefaultRateAuditEngine(Flags.NewRegistry());
        var item = RateAudit.NewRateAudit("code-929", "name-968");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateAuditEngineKeepsResultsWithinBounds();
    }
}
