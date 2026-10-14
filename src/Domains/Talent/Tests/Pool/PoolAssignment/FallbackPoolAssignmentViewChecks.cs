namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class FallbackPoolAssignmentViewChecks
{
    public static void FallbackPoolAssignmentViewShowsTheStatusInCapitals()
    {
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("code-293", "name-132");
        var view = FallbackPoolAssignmentView.ToFallbackPoolAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPoolAssignmentView.ToFallbackPoolAssignmentViews(new List<FallbackPoolAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPoolAssignmentViewShowsTheStatusInCapitals();
    }
}
