namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedSpanDigestReportChecks
{
    public static void CappedSpanDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700529210);
        var items = new List<CappedSpanDigest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedSpanDigest.NewCappedSpanDigest($"capped-span-digest-{index}", "sample"));
        }
        var report = CappedSpanDigestReport.BuildCappedSpanDigestReport("test", items, clock);
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
        CappedSpanDigestReportCountsEachRecordOnce();
    }
}
