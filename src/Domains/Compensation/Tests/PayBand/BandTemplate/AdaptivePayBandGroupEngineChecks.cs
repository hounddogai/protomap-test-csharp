namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class AdaptivePayBandGroupEngineChecks
{
    public static void AdaptivePayBandGroupEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePayBandGroupEngine.DefaultAdaptivePayBandGroupEngine(Flags.NewRegistry());
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("code-876", "title-841");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePayBandGroupEngineKeepsResultsWithinBounds();
    }
}
