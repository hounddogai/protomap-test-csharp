namespace Acme.Workforce.Compliance.Tests.Exception.Proposal;

using Acme.Workforce.Compliance.Exception.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ExceptionOverrideReportChecks
{
    public static void ExceptionOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709612794);
        var items = new List<ExceptionOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ExceptionOverride.NewExceptionOverride($"exception-override-{index}", "sample"));
        }
        var report = ExceptionOverrideReport.BuildExceptionOverrideReport("test", items, clock);
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
        ExceptionOverrideReportCountsEachRecordOnce();
    }
}
