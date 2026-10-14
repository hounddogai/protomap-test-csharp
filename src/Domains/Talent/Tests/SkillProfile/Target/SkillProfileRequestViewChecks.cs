namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfileRequestViewChecks
{
    public static void SkillProfileRequestViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileRequest.NewSkillProfileRequest("code-538", "name-873");
        var view = SkillProfileRequestView.ToSkillProfileRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileRequestView.ToSkillProfileRequestViews(new List<SkillProfileRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileRequestViewShowsTheStatusInCapitals();
    }
}
