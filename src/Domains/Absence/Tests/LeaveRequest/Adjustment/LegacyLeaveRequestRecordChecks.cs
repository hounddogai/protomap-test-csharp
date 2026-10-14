namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyLeaveRequestRecordChecks
{
    public static void LegacyLeaveRequestRecordStartsInNew()
    {
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("code-592", "name-332");
        Check.Equal(item.Status, LegacyLeaveRequestRecordStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyLeaveRequestRecordMovesToFailed()
    {
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("code-592", "name-332");
        try
        {
            item.MoveTo(LegacyLeaveRequestRecordStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyLeaveRequestRecordStatus.Failed, "status");
    }

    public static void LegacyLeaveRequestRecordRejectsMovesFromFailed()
    {
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("code-592", "name-332");
        item.Status = LegacyLeaveRequestRecordStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyLeaveRequestRecordStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestRecordStartsInNew();
        LegacyLeaveRequestRecordMovesToFailed();
        LegacyLeaveRequestRecordRejectsMovesFromFailed();
    }
}
