namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RollingRecognitionReservationReportChecks
{
    public static void RollingRecognitionReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709590576);
        var items = new List<RollingRecognitionReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingRecognitionReservation.NewRollingRecognitionReservation($"rolling-recognition-reservation-{index}", "sample"));
        }
        var report = RollingRecognitionReservationReport.BuildRollingRecognitionReservationReport("test", items, clock);
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
        RollingRecognitionReservationReportCountsEachRecordOnce();
    }
}
