namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackSkillProfileTierViewChecks
{
    public static void FallbackSkillProfileTierViewShowsTheStatusInCapitals()
    {
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("code-567", "name-193");
        var view = FallbackSkillProfileTierView.ToFallbackSkillProfileTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackSkillProfileTierView.ToFallbackSkillProfileTierViews(new List<FallbackSkillProfileTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackSkillProfileTierViewShowsTheStatusInCapitals();
    }
}
