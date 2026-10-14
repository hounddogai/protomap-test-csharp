namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationMappingViewChecks
{
    public static void NominationMappingViewShowsTheStatusInCapitals()
    {
        var item = NominationMapping.NewNominationMapping("code-742", "name-237");
        var view = NominationMappingView.ToNominationMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationMappingView.ToNominationMappingViews(new List<NominationMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationMappingViewShowsTheStatusInCapitals();
    }
}
