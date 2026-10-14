namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatRecordChecks
{
    public static void LicenseSeatRecordStartsInRequested()
    {
        var item = LicenseSeatRecord.NewLicenseSeatRecord("code-274", "name-824");
        Check.Equal(item.Status, LicenseSeatRecordStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatRecordMovesToGranted()
    {
        var item = LicenseSeatRecord.NewLicenseSeatRecord("code-274", "name-824");
        try
        {
            item.MoveTo(LicenseSeatRecordStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatRecordStatus.Granted, "status");
    }

    public static void LicenseSeatRecordRejectsMovesFromExpired()
    {
        var item = LicenseSeatRecord.NewLicenseSeatRecord("code-274", "name-824");
        item.Status = LicenseSeatRecordStatus.Expired;
        Check.Fails(() => item.MoveTo(LicenseSeatRecordStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LicenseSeatRecordStartsInRequested();
        LicenseSeatRecordMovesToGranted();
        LicenseSeatRecordRejectsMovesFromExpired();
    }
}
