namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationNoticeViewChecks
{
    public static void NominationNoticeViewShowsTheStatusInCapitals()
    {
        var item = NominationNotice.NewNominationNotice("code-930", "title-886");
        var view = NominationNoticeView.ToNominationNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationNoticeView.ToNominationNoticeViews(new List<NominationNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationNoticeViewShowsTheStatusInCapitals();
    }
}
