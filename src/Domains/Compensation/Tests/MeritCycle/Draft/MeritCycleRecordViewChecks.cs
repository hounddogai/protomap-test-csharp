namespace Acme.Workforce.Compensation.Tests.MeritCycle.Draft;

using Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class MeritCycleRecordViewChecks
{
    public static void MeritCycleRecordViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleRecord.NewMeritCycleRecord("code-193", "name-322");
        var view = MeritCycleRecordView.ToMeritCycleRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleRecordView.ToMeritCycleRecordViews(new List<MeritCycleRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleRecordViewShowsTheStatusInCapitals();
    }
}
