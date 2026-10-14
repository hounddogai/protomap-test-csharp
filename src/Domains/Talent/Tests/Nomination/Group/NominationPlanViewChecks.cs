namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationPlanViewChecks
{
    public static void NominationPlanViewShowsTheStatusInCapitals()
    {
        var item = NominationPlan.NewNominationPlan("code-127", "label-252");
        var view = NominationPlanView.ToNominationPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationPlanView.ToNominationPlanViews(new List<NominationPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationPlanViewShowsTheStatusInCapitals();
    }
}
