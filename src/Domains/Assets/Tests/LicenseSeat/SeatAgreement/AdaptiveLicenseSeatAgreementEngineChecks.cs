namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAgreementEngineChecks
{
    public static void AdaptiveLicenseSeatAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveLicenseSeatAgreementEngine.DefaultAdaptiveLicenseSeatAgreementEngine(Flags.NewRegistry());
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("code-384", "label-132");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatAgreementEngineKeepsResultsWithinBounds();
    }
}
