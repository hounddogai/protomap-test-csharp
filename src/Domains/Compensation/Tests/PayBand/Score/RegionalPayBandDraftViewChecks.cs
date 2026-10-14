namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RegionalPayBandDraftViewChecks
{
    public static void RegionalPayBandDraftViewShowsTheStatusInCapitals()
    {
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("code-971", "title-999");
        var view = RegionalPayBandDraftView.ToRegionalPayBandDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalPayBandDraftView.ToRegionalPayBandDraftViews(new List<RegionalPayBandDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalPayBandDraftViewShowsTheStatusInCapitals();
    }
}
