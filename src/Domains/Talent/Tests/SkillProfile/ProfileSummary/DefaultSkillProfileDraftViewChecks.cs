namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class DefaultSkillProfileDraftViewChecks
{
    public static void DefaultSkillProfileDraftViewShowsTheStatusInCapitals()
    {
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("code-307", "name-985");
        var view = DefaultSkillProfileDraftView.ToDefaultSkillProfileDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultSkillProfileDraftView.ToDefaultSkillProfileDraftViews(new List<DefaultSkillProfileDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultSkillProfileDraftViewShowsTheStatusInCapitals();
    }
}
