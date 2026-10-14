namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AdaptiveLeaveRequestPolicyEngineChecks
{
    public static void AdaptiveLeaveRequestPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveLeaveRequestPolicyEngine.DefaultAdaptiveLeaveRequestPolicyEngine();
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("code-562", "name-723");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveLeaveRequestPolicyEngineKeepsResultsWithinBounds();
    }
}
