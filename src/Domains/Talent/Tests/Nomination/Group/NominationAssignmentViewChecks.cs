namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationAssignmentViewChecks
{
    public static void NominationAssignmentViewShowsTheStatusInCapitals()
    {
        var item = NominationAssignment.NewNominationAssignment("code-441", "label-121");
        var view = NominationAssignmentView.ToNominationAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationAssignmentView.ToNominationAssignmentViews(new List<NominationAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationAssignmentViewShowsTheStatusInCapitals();
    }
}
