namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class SkillProfileQuotaViewChecks
{
    public static void SkillProfileQuotaViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileQuota.NewSkillProfileQuota("code-91", "label-572");
        var view = SkillProfileQuotaView.ToSkillProfileQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileQuotaView.ToSkillProfileQuotaViews(new List<SkillProfileQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileQuotaViewShowsTheStatusInCapitals();
    }
}
