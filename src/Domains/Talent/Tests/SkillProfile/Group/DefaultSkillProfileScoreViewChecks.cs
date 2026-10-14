namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultSkillProfileScoreViewChecks
{
    public static void DefaultSkillProfileScoreViewShowsTheStatusInCapitals()
    {
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("code-868", "label-883");
        var view = DefaultSkillProfileScoreView.ToDefaultSkillProfileScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultSkillProfileScoreView.ToDefaultSkillProfileScoreViews(new List<DefaultSkillProfileScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultSkillProfileScoreViewShowsTheStatusInCapitals();
    }
}
