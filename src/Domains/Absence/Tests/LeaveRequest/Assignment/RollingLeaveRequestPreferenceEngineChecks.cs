namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class RollingLeaveRequestPreferenceEngineChecks
{
    public static void RollingLeaveRequestPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = RollingLeaveRequestPreferenceEngine.DefaultRollingLeaveRequestPreferenceEngine(Flags.NewRegistry());
        var item = RollingLeaveRequestPreference.NewRollingLeaveRequestPreference("code-768", "label-356");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingLeaveRequestPreferenceEngineKeepsResultsWithinBounds();
    }
}
