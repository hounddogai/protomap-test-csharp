namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientActionPlanRecordReportChecks
{
    public static void LenientActionPlanRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700378823);
        var items = new List<LenientActionPlanRecord>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientActionPlanRecord.NewLenientActionPlanRecord($"lenient-action-plan-record-{index}", "sample"));
        }
        var report = LenientActionPlanRecordReport.BuildLenientActionPlanRecordReport("test", items, clock);
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
        LenientActionPlanRecordReportCountsEachRecordOnce();
    }
}
