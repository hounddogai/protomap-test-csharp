namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveLeaveRequestLedgerEngineChecks
{
    public static void AdaptiveLeaveRequestLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveLeaveRequestLedgerEngine.DefaultAdaptiveLeaveRequestLedgerEngine();
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("code-353", "name-720");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveLeaveRequestLedgerEngineKeepsResultsWithinBounds();
    }
}
