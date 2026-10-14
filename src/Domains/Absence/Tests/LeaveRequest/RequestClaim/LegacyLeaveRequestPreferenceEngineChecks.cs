namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LegacyLeaveRequestPreferenceEngineChecks
{
    public static void LegacyLeaveRequestPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyLeaveRequestPreferenceEngine.DefaultLegacyLeaveRequestPreferenceEngine(Flags.NewRegistry());
        var item = LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference("code-598", "label-782");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestPreferenceEngineKeepsResultsWithinBounds();
    }
}
