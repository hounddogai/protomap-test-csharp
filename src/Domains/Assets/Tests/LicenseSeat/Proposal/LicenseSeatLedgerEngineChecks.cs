namespace Acme.Workforce.Assets.Tests.LicenseSeat.Proposal;

using Acme.Workforce.Assets.LicenseSeat.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LicenseSeatLedgerEngineChecks
{
    public static void LicenseSeatLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatLedgerEngine.DefaultLicenseSeatLedgerEngine(Flags.NewRegistry());
        var item = LicenseSeatLedger.NewLicenseSeatLedger("code-34", "label-399");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatLedgerEngineKeepsResultsWithinBounds();
    }
}
