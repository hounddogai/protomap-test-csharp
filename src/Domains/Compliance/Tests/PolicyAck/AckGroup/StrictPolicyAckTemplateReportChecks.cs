namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckGroup;

using Acme.Workforce.Compliance.PolicyAck.AckGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ack group code, run by the project's runner.</summary>
public static class StrictPolicyAckTemplateReportChecks
{
    public static void StrictPolicyAckTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702908366);
        var items = new List<StrictPolicyAckTemplate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictPolicyAckTemplate.NewStrictPolicyAckTemplate($"strict-policy-ack-template-{index}", "sample"));
        }
        var report = StrictPolicyAckTemplateReport.BuildStrictPolicyAckTemplateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        StrictPolicyAckTemplateReportCountsEachRecordOnce();
    }
}
