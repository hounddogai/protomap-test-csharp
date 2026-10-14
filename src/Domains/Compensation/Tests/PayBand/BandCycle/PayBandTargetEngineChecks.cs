namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandTargetEngineChecks
{
    public static void PayBandTargetEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandTargetEngine.DefaultPayBandTargetEngine();
        var item = PayBandTarget.NewPayBandTarget("code-551", "name-611");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandTargetEngineKeepsResultsWithinBounds();
    }
}
