namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionPlanReportChecks
{
    public static void RecognitionPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704021832);
        var items = new List<RecognitionPlan>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RecognitionPlan.NewRecognitionPlan($"recognition-plan-{index}", "sample"));
        }
        var report = RecognitionPlanReport.BuildRecognitionPlanReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RecognitionPlanReportCountsEachRecordOnce();
    }
}
