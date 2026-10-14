namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandRollupEngineChecks
{
    public static void PayBandRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandRollupEngine.DefaultPayBandRollupEngine(Flags.NewRegistry());
        var item = PayBandRollup.NewPayBandRollup("code-635", "title-282");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandRollupEngineKeepsResultsWithinBounds();
    }
}
