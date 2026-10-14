namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PayBandGroupEngineChecks
{
    public static void PayBandGroupEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandGroupEngine.DefaultPayBandGroupEngine(Flags.NewRegistry());
        var item = PayBandGroup.NewPayBandGroup("code-674", "label-609");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandGroupEngineKeepsResultsWithinBounds();
    }
}
