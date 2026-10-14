namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class CappedNominationWindowViewChecks
{
    public static void CappedNominationWindowViewShowsTheStatusInCapitals()
    {
        var item = CappedNominationWindow.NewCappedNominationWindow("code-65", "label-799");
        var view = CappedNominationWindowView.ToCappedNominationWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedNominationWindowView.ToCappedNominationWindowViews(new List<CappedNominationWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedNominationWindowViewShowsTheStatusInCapitals();
    }
}
