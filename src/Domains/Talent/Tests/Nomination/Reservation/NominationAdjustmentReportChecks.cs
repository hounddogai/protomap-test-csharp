namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationAdjustmentReportChecks
{
    public static void NominationAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705554628);
        var items = new List<NominationAdjustment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(NominationAdjustment.NewNominationAdjustment($"nomination-adjustment-{index}", "sample"));
        }
        var report = NominationAdjustmentReport.BuildNominationAdjustmentReport("test", items, clock);
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
        NominationAdjustmentReportCountsEachRecordOnce();
    }
}
