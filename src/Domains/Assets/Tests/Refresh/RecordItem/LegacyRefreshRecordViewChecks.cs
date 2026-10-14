namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyRefreshRecordViewChecks
{
    public static void LegacyRefreshRecordViewShowsTheStatusInCapitals()
    {
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("code-726", "label-838");
        var view = LegacyRefreshRecordView.ToLegacyRefreshRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyRefreshRecordView.ToLegacyRefreshRecordViews(new List<LegacyRefreshRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyRefreshRecordViewShowsTheStatusInCapitals();
    }
}
