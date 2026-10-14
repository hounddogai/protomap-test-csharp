namespace Acme.Workforce.Talent.Tests.SkillProfile.Score;

using Acme.Workforce.Talent.SkillProfile.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SkillProfileExceptionViewChecks
{
    public static void SkillProfileExceptionViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileException.NewSkillProfileException("code-904", "title-898");
        var view = SkillProfileExceptionView.ToSkillProfileExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileExceptionView.ToSkillProfileExceptionViews(new List<SkillProfileException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileExceptionViewShowsTheStatusInCapitals();
    }
}
