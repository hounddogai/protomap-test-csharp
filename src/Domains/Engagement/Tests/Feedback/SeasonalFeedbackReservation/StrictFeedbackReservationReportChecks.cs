namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class StrictFeedbackReservationReportChecks
{
    public static void StrictFeedbackReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705098571);
        var items = new List<StrictFeedbackReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictFeedbackReservation.NewStrictFeedbackReservation($"strict-feedback-reservation-{index}", "sample"));
        }
        var report = StrictFeedbackReservationReport.BuildStrictFeedbackReservationReport("test", items, clock);
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
        StrictFeedbackReservationReportCountsEachRecordOnce();
    }
}
