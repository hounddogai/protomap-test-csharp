namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyDraftViewChecks
{
    public static void OccupancyDraftViewShowsTheStatusInCapitals()
    {
        var item = OccupancyDraft.NewOccupancyDraft("code-831", "label-508");
        var view = OccupancyDraftView.ToOccupancyDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyDraftView.ToOccupancyDraftViews(new List<OccupancyDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyDraftViewShowsTheStatusInCapitals();
    }
}
