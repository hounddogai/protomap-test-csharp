namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class CappedPositionAssignmentViewChecks
{
    public static void CappedPositionAssignmentViewShowsTheStatusInCapitals()
    {
        var item = CappedPositionAssignment.NewCappedPositionAssignment("code-379", "name-69");
        var view = CappedPositionAssignmentView.ToCappedPositionAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPositionAssignmentView.ToCappedPositionAssignmentViews(new List<CappedPositionAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPositionAssignmentViewShowsTheStatusInCapitals();
    }
}
