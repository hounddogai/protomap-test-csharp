namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScheduledSkillProfilePolicyViewChecks
{
    public static void ScheduledSkillProfilePolicyViewShowsTheStatusInCapitals()
    {
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("code-84", "label-310");
        var view = ScheduledSkillProfilePolicyView.ToScheduledSkillProfilePolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledSkillProfilePolicyView.ToScheduledSkillProfilePolicyViews(new List<ScheduledSkillProfilePolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledSkillProfilePolicyViewShowsTheStatusInCapitals();
    }
}
