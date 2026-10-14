namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalPayBandAgreementEngineChecks
{
    public static void GlobalPayBandAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPayBandAgreementEngine.DefaultGlobalPayBandAgreementEngine();
        var item = GlobalPayBandAgreement.NewGlobalPayBandAgreement("code-32", "label-144");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPayBandAgreementEngineKeepsResultsWithinBounds();
    }
}
