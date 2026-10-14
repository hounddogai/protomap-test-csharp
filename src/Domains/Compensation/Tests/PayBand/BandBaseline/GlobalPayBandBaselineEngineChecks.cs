namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class GlobalPayBandBaselineEngineChecks
{
    public static void GlobalPayBandBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPayBandBaselineEngine.DefaultGlobalPayBandBaselineEngine(Flags.NewRegistry());
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("code-754", "name-319");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPayBandBaselineEngineKeepsResultsWithinBounds();
    }
}
