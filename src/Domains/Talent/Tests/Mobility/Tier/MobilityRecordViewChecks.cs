namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityRecordViewChecks
{
    public static void MobilityRecordViewShowsTheStatusInCapitals()
    {
        var item = MobilityRecord.NewMobilityRecord("code-549", "title-859");
        var view = MobilityRecordView.ToMobilityRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityRecordView.ToMobilityRecordViews(new List<MobilityRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityRecordViewShowsTheStatusInCapitals();
    }
}
