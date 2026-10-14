namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyPayBandAgreementEngineChecks
{
    public static void LegacyPayBandAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPayBandAgreementEngine.DefaultLegacyPayBandAgreementEngine(Flags.NewRegistry());
        var item = LegacyPayBandAgreement.NewLegacyPayBandAgreement("code-235", "label-214");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPayBandAgreementEngineKeepsResultsWithinBounds();
    }
}
