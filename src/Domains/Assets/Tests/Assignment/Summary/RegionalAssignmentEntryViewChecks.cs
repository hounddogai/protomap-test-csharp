namespace Acme.Workforce.Assets.Tests.Assignment.Summary;

using Acme.Workforce.Assets.Assignment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RegionalAssignmentEntryViewChecks
{
    public static void RegionalAssignmentEntryViewShowsTheStatusInCapitals()
    {
        var item = RegionalAssignmentEntry.NewRegionalAssignmentEntry("code-91", "title-244");
        var view = RegionalAssignmentEntryView.ToRegionalAssignmentEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalAssignmentEntryView.ToRegionalAssignmentEntryViews(new List<RegionalAssignmentEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalAssignmentEntryViewShowsTheStatusInCapitals();
    }
}
