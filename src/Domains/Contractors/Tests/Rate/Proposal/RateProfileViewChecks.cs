namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RateProfileViewChecks
{
    public static void RateProfileViewShowsTheStatusInCapitals()
    {
        var item = RateProfile.NewRateProfile("code-138", "title-450");
        var view = RateProfileView.ToRateProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateProfileView.ToRateProfileViews(new List<RateProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateProfileViewShowsTheStatusInCapitals();
    }
}
