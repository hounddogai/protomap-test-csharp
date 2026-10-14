namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestClaimEngineChecks
{
    public static void PrimaryLeaveRequestClaimEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryLeaveRequestClaimEngine.DefaultPrimaryLeaveRequestClaimEngine(Flags.NewRegistry());
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("code-429", "name-211");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestClaimEngineKeepsResultsWithinBounds();
    }
}
