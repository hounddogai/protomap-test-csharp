namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class CappedSkillProfileMappingViewChecks
{
    public static void CappedSkillProfileMappingViewShowsTheStatusInCapitals()
    {
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("code-284", "title-401");
        var view = CappedSkillProfileMappingView.ToCappedSkillProfileMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedSkillProfileMappingView.ToCappedSkillProfileMappingViews(new List<CappedSkillProfileMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedSkillProfileMappingViewShowsTheStatusInCapitals();
    }
}
