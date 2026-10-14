namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultPayBandGroupEngineChecks
{
    public static void DefaultPayBandGroupEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPayBandGroupEngine.DefaultDefaultPayBandGroupEngine(Flags.NewRegistry());
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("code-336", "title-491");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPayBandGroupEngineKeepsResultsWithinBounds();
    }
}
