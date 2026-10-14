namespace Acme.Workforce.Talent.Tests.Nomination.Target;

using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class NominationTargetViewChecks
{
    public static void NominationTargetViewShowsTheStatusInCapitals()
    {
        var item = NominationTarget.NewNominationTarget("code-512", "name-19");
        var view = NominationTargetView.ToNominationTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationTargetView.ToNominationTargetViews(new List<NominationTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationTargetViewShowsTheStatusInCapitals();
    }
}
