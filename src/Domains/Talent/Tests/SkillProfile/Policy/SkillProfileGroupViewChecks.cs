namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileGroupViewChecks
{
    public static void SkillProfileGroupViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileGroup.NewSkillProfileGroup("code-986", "name-326");
        var view = SkillProfileGroupView.ToSkillProfileGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileGroupView.ToSkillProfileGroupViews(new List<SkillProfileGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileGroupViewShowsTheStatusInCapitals();
    }
}
