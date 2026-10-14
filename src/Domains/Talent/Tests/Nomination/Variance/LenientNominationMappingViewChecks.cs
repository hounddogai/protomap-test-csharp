namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LenientNominationMappingViewChecks
{
    public static void LenientNominationMappingViewShowsTheStatusInCapitals()
    {
        var item = LenientNominationMapping.NewLenientNominationMapping("code-362", "name-109");
        var view = LenientNominationMappingView.ToLenientNominationMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientNominationMappingView.ToLenientNominationMappingViews(new List<LenientNominationMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientNominationMappingViewShowsTheStatusInCapitals();
    }
}
