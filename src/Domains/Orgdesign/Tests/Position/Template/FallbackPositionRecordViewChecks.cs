namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class FallbackPositionRecordViewChecks
{
    public static void FallbackPositionRecordViewShowsTheStatusInCapitals()
    {
        var item = FallbackPositionRecord.NewFallbackPositionRecord("code-288", "label-951");
        var view = FallbackPositionRecordView.ToFallbackPositionRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPositionRecordView.ToFallbackPositionRecordViews(new List<FallbackPositionRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPositionRecordViewShowsTheStatusInCapitals();
    }
}
