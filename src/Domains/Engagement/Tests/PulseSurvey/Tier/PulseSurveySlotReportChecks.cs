namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PulseSurveySlotReportChecks
{
    public static void PulseSurveySlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702831466);
        var items = new List<PulseSurveySlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PulseSurveySlot.NewPulseSurveySlot($"pulse-survey-slot-{index}", "sample"));
        }
        var report = PulseSurveySlotReport.BuildPulseSurveySlotReport("test", items, clock);
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
        PulseSurveySlotReportCountsEachRecordOnce();
    }
}
