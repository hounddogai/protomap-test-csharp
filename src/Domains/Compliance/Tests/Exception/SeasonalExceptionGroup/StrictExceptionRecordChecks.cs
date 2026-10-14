namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class StrictExceptionRecordChecks
{
    public static void StrictExceptionRecordStartsInRequested()
    {
        var item = StrictExceptionRecord.NewStrictExceptionRecord("code-193", "title-247");
        Check.Equal(item.Status, StrictExceptionRecordStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictExceptionRecordMovesToExpired()
    {
        var item = StrictExceptionRecord.NewStrictExceptionRecord("code-193", "title-247");
        try
        {
            item.MoveTo(StrictExceptionRecordStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictExceptionRecordStatus.Expired, "status");
    }

    public static void StrictExceptionRecordRejectsMovesFromExpired()
    {
        var item = StrictExceptionRecord.NewStrictExceptionRecord("code-193", "title-247");
        item.Status = StrictExceptionRecordStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictExceptionRecordStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictExceptionRecordStartsInRequested();
        StrictExceptionRecordMovesToExpired();
        StrictExceptionRecordRejectsMovesFromExpired();
    }
}
