namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalMobilityRecordViewChecks
{
    public static void SeasonalMobilityRecordViewShowsTheStatusInCapitals()
    {
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("code-529", "label-692");
        var view = SeasonalMobilityRecordView.ToSeasonalMobilityRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalMobilityRecordView.ToSeasonalMobilityRecordViews(new List<SeasonalMobilityRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalMobilityRecordViewShowsTheStatusInCapitals();
    }
}
