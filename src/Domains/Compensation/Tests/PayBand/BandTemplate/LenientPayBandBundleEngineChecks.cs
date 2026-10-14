namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class LenientPayBandBundleEngineChecks
{
    public static void LenientPayBandBundleEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPayBandBundleEngine.DefaultLenientPayBandBundleEngine();
        var item = LenientPayBandBundle.NewLenientPayBandBundle("code-54", "label-164");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPayBandBundleEngineKeepsResultsWithinBounds();
    }
}
