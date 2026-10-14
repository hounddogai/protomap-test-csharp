namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyLeaveRequestRecordEngineChecks
{
    public static void LegacyLeaveRequestRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyLeaveRequestRecordEngine.DefaultLegacyLeaveRequestRecordEngine();
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("code-592", "name-332");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestRecordEngineKeepsResultsWithinBounds();
    }
}
