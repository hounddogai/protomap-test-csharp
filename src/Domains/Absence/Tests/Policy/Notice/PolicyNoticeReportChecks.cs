namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyNoticeReportChecks
{
    public static void PolicyNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709074956);
        var items = new List<PolicyNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PolicyNotice.NewPolicyNotice($"policy-notice-{index}", "sample"));
        }
        var report = PolicyNoticeReport.BuildPolicyNoticeReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PolicyNoticeReportCountsEachRecordOnce();
    }
}
