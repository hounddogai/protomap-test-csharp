namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationScheduleViewChecks
{
    public static void NominationScheduleViewShowsTheStatusInCapitals()
    {
        var item = NominationSchedule.NewNominationSchedule("code-324", "name-266");
        var view = NominationScheduleView.ToNominationScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationScheduleView.ToNominationScheduleViews(new List<NominationSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationScheduleViewShowsTheStatusInCapitals();
    }
}
