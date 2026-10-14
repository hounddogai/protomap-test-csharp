namespace Acme.Workforce.Talent.Tests.SkillProfile.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SkillProfileAgreementReportChecks
{
    public static void SkillProfileAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706534713);
        var items = new List<SkillProfileAgreement>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SkillProfileAgreement.NewSkillProfileAgreement($"skill-profile-agreement-{index}", "sample"));
        }
        var report = SkillProfileAgreementReport.BuildSkillProfileAgreementReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SkillProfileAgreementReportCountsEachRecordOnce();
    }
}
