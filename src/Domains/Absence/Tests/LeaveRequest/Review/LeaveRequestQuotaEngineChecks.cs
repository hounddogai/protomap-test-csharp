namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LeaveRequestQuotaEngineChecks
{
    public static void LeaveRequestQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestQuotaEngine.DefaultLeaveRequestQuotaEngine(Flags.NewRegistry());
        var item = LeaveRequestQuota.NewLeaveRequestQuota("code-252", "name-154");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestQuotaEngineKeepsResultsWithinBounds();
    }
}
