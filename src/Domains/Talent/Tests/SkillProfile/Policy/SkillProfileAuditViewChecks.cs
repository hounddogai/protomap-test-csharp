namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileAuditViewChecks
{
    public static void SkillProfileAuditViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileAudit.NewSkillProfileAudit("code-741", "title-493");
        var view = SkillProfileAuditView.ToSkillProfileAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileAuditView.ToSkillProfileAuditViews(new List<SkillProfileAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileAuditViewShowsTheStatusInCapitals();
    }
}
