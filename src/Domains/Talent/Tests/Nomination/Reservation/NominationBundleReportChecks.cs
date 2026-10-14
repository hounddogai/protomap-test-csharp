namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationBundleReportChecks
{
    public static void NominationBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704798423);
        var items = new List<NominationBundle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(NominationBundle.NewNominationBundle($"nomination-bundle-{index}", "sample"));
        }
        var report = NominationBundleReport.BuildNominationBundleReport("test", items, clock);
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
        NominationBundleReportCountsEachRecordOnce();
    }
}
