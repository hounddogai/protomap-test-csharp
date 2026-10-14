namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyReservationReportChecks
{
    public static void PulseSurveyReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705607882);
        var items = new List<PulseSurveyReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PulseSurveyReservation.NewPulseSurveyReservation($"pulse-survey-reservation-{index}", "sample"));
        }
        var report = PulseSurveyReservationReport.BuildPulseSurveyReservationReport("test", items, clock);
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
        PulseSurveyReservationReportCountsEachRecordOnce();
    }
}
