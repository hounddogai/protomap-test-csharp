namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatRecordViewChecks
{
    public static void LicenseSeatRecordViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatRecord.NewLicenseSeatRecord("code-274", "name-824");
        var view = LicenseSeatRecordView.ToLicenseSeatRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatRecordView.ToLicenseSeatRecordViews(new List<LicenseSeatRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatRecordViewShowsTheStatusInCapitals();
    }
}
