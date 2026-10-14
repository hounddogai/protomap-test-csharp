namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionReservationReportChecks
{
    public static void RecognitionReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700067535);
        var items = new List<RecognitionReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RecognitionReservation.NewRecognitionReservation($"recognition-reservation-{index}", "sample"));
        }
        var report = RecognitionReservationReport.BuildRecognitionReservationReport("test", items, clock);
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
        RecognitionReservationReportCountsEachRecordOnce();
    }
}
