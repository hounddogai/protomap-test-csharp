namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementRecordViewChecks
{
    public static void EngagementRecordViewShowsTheStatusInCapitals()
    {
        var item = EngagementRecord.NewEngagementRecord("code-629", "title-18");
        var view = EngagementRecordView.ToEngagementRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementRecordView.ToEngagementRecordViews(new List<EngagementRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementRecordViewShowsTheStatusInCapitals();
    }
}
