namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class MobilityAssignmentViewChecks
{
    public static void MobilityAssignmentViewShowsTheStatusInCapitals()
    {
        var item = MobilityAssignment.NewMobilityAssignment("code-254", "label-78");
        var view = MobilityAssignmentView.ToMobilityAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityAssignmentView.ToMobilityAssignmentViews(new List<MobilityAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityAssignmentViewShowsTheStatusInCapitals();
    }
}
