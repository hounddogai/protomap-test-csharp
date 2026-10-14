namespace Acme.Workforce.Engagement.Tests.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PulseSurveySnapshotReportChecks
{
    public static void PulseSurveySnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704702138);
        var items = new List<PulseSurveySnapshot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PulseSurveySnapshot.NewPulseSurveySnapshot($"pulse-survey-snapshot-{index}", "sample"));
        }
        var report = PulseSurveySnapshotReport.BuildPulseSurveySnapshotReport("test", items, clock);
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
        PulseSurveySnapshotReportCountsEachRecordOnce();
    }
}
