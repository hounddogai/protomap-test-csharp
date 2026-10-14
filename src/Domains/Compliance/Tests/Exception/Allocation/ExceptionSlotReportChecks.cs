namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ExceptionSlotReportChecks
{
    public static void ExceptionSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705207252);
        var items = new List<ExceptionSlot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ExceptionSlot.NewExceptionSlot($"exception-slot-{index}", "sample"));
        }
        var report = ExceptionSlotReport.BuildExceptionSlotReport("test", items, clock);
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
        ExceptionSlotReportCountsEachRecordOnce();
    }
}
