namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class GlobalMobilityRecordViewChecks
{
    public static void GlobalMobilityRecordViewShowsTheStatusInCapitals()
    {
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("code-788", "title-940");
        var view = GlobalMobilityRecordView.ToGlobalMobilityRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalMobilityRecordView.ToGlobalMobilityRecordViews(new List<GlobalMobilityRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalMobilityRecordViewShowsTheStatusInCapitals();
    }
}
