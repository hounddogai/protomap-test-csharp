namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredOccupancyRecordViewChecks
{
    public static void TieredOccupancyRecordViewShowsTheStatusInCapitals()
    {
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("code-596", "label-939");
        var view = TieredOccupancyRecordView.ToTieredOccupancyRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredOccupancyRecordView.ToTieredOccupancyRecordViews(new List<TieredOccupancyRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredOccupancyRecordViewShowsTheStatusInCapitals();
    }
}
