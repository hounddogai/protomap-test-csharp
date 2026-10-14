namespace Acme.Workforce.Orgdesign.Tests.Position.Manifest;

using Acme.Workforce.Orgdesign.Position.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LegacyPositionAssignmentViewChecks
{
    public static void LegacyPositionAssignmentViewShowsTheStatusInCapitals()
    {
        var item = LegacyPositionAssignment.NewLegacyPositionAssignment("code-352", "title-542");
        var view = LegacyPositionAssignmentView.ToLegacyPositionAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPositionAssignmentView.ToLegacyPositionAssignmentViews(new List<LegacyPositionAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPositionAssignmentViewShowsTheStatusInCapitals();
    }
}
