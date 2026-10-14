namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultMobilityRecordViewChecks
{
    public static void DefaultMobilityRecordViewShowsTheStatusInCapitals()
    {
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("code-121", "label-14");
        var view = DefaultMobilityRecordView.ToDefaultMobilityRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultMobilityRecordView.ToDefaultMobilityRecordViews(new List<DefaultMobilityRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultMobilityRecordViewShowsTheStatusInCapitals();
    }
}
