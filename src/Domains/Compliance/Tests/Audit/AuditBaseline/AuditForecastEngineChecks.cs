namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class AuditForecastEngineChecks
{
    public static void AuditForecastEngineKeepsResultsWithinBounds()
    {
        var engine = AuditForecastEngine.DefaultAuditForecastEngine(Flags.NewRegistry());
        var item = AuditForecast.NewAuditForecast("code-114", "name-134");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditForecastEngineKeepsResultsWithinBounds();
    }
}
