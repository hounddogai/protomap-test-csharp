namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LeaveRequestClaimEngineChecks
{
    public static void LeaveRequestClaimEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestClaimEngine.DefaultLeaveRequestClaimEngine();
        var item = LeaveRequestClaim.NewLeaveRequestClaim("code-662", "title-223");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestClaimEngineKeepsResultsWithinBounds();
    }
}
