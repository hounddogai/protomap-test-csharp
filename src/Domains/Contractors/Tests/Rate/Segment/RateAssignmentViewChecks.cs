namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateAssignmentViewChecks
{
    public static void RateAssignmentViewShowsTheStatusInCapitals()
    {
        var item = RateAssignment.NewRateAssignment("code-541", "title-170");
        var view = RateAssignmentView.ToRateAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateAssignmentView.ToRateAssignmentViews(new List<RateAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateAssignmentViewShowsTheStatusInCapitals();
    }
}
