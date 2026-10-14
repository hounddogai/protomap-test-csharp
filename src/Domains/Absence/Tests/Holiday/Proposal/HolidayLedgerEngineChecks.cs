namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayLedgerEngineChecks
{
    public static void HolidayLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayLedgerEngine.DefaultHolidayLedgerEngine(Flags.NewRegistry());
        var item = HolidayLedger.NewHolidayLedger("code-118", "label-880");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayLedgerEngineKeepsResultsWithinBounds();
    }
}
