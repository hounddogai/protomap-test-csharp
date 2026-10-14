namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationMilestoneViewChecks
{
    public static void NominationMilestoneViewShowsTheStatusInCapitals()
    {
        var item = NominationMilestone.NewNominationMilestone("code-82", "title-722");
        var view = NominationMilestoneView.ToNominationMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationMilestoneView.ToNominationMilestoneViews(new List<NominationMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationMilestoneViewShowsTheStatusInCapitals();
    }
}
