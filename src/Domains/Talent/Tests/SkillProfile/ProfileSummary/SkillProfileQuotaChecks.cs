namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class SkillProfileQuotaChecks
{
    public static void SkillProfileQuotaStartsInPlanned()
    {
        var item = SkillProfileQuota.NewSkillProfileQuota("code-91", "label-572");
        Check.Equal(item.Status, SkillProfileQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileQuotaMovesToScheduled()
    {
        var item = SkillProfileQuota.NewSkillProfileQuota("code-91", "label-572");
        try
        {
            item.MoveTo(SkillProfileQuotaStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileQuotaStatus.Scheduled, "status");
    }

    public static void SkillProfileQuotaRejectsMovesFromCancelled()
    {
        var item = SkillProfileQuota.NewSkillProfileQuota("code-91", "label-572");
        item.Status = SkillProfileQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SkillProfileQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SkillProfileQuotaStartsInPlanned();
        SkillProfileQuotaMovesToScheduled();
        SkillProfileQuotaRejectsMovesFromCancelled();
    }
}
