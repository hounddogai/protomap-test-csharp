namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class PayBandRuleEngineChecks
{
    public static void PayBandRuleEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandRuleEngine.DefaultPayBandRuleEngine();
        var item = PayBandRule.NewPayBandRule("code-811", "label-612");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandRuleEngineKeepsResultsWithinBounds();
    }
}
