namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RegionalSkillProfileGroupViewChecks
{
    public static void RegionalSkillProfileGroupViewShowsTheStatusInCapitals()
    {
        var item = RegionalSkillProfileGroup.NewRegionalSkillProfileGroup("code-621", "label-205");
        var view = RegionalSkillProfileGroupView.ToRegionalSkillProfileGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalSkillProfileGroupView.ToRegionalSkillProfileGroupViews(new List<RegionalSkillProfileGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalSkillProfileGroupViewShowsTheStatusInCapitals();
    }
}
