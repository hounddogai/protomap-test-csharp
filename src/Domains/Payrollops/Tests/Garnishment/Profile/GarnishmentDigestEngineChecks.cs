namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentDigestEngineChecks
{
    public static void GarnishmentDigestEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentDigestEngine.DefaultGarnishmentDigestEngine();
        var item = GarnishmentDigest.NewGarnishmentDigest("code-833", "label-310");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentDigestEngineKeepsResultsWithinBounds();
    }
}
