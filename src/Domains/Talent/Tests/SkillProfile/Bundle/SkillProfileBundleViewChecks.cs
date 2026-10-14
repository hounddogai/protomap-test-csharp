namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SkillProfileBundleViewChecks
{
    public static void SkillProfileBundleViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileBundle.NewSkillProfileBundle("code-230", "label-715");
        var view = SkillProfileBundleView.ToSkillProfileBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileBundleView.ToSkillProfileBundleViews(new List<SkillProfileBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileBundleViewShowsTheStatusInCapitals();
    }
}
