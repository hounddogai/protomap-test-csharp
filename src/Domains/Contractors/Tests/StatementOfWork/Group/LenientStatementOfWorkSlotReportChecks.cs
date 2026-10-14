namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Group;

using Acme.Workforce.Contractors.StatementOfWork.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LenientStatementOfWorkSlotReportChecks
{
    public static void LenientStatementOfWorkSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702626482);
        var items = new List<LenientStatementOfWorkSlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot($"lenient-statement-of-work-slot-{index}", "sample"));
        }
        var report = LenientStatementOfWorkSlotReport.BuildLenientStatementOfWorkSlotReport("test", items, clock);
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
        LenientStatementOfWorkSlotReportCountsEachRecordOnce();
    }
}
