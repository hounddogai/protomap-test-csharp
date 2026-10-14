namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class DefaultExceptionDigestReportChecks
{
    public static void DefaultExceptionDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704327851);
        var items = new List<DefaultExceptionDigest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultExceptionDigest.NewDefaultExceptionDigest($"default-exception-digest-{index}", "sample"));
        }
        var report = DefaultExceptionDigestReport.BuildDefaultExceptionDigestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultExceptionDigestReportCountsEachRecordOnce();
    }
}
