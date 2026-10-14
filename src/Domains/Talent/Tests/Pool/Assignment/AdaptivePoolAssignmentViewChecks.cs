namespace Acme.Workforce.Talent.Tests.Pool.Assignment;

using Acme.Workforce.Talent.Pool.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AdaptivePoolAssignmentViewChecks
{
    public static void AdaptivePoolAssignmentViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("code-311", "label-333");
        var view = AdaptivePoolAssignmentView.ToAdaptivePoolAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePoolAssignmentView.ToAdaptivePoolAssignmentViews(new List<AdaptivePoolAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePoolAssignmentViewShowsTheStatusInCapitals();
    }
}
