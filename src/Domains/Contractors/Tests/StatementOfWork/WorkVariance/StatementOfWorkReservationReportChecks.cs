namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of work variance code, run by the project's runner.</summary>
public static class StatementOfWorkReservationReportChecks
{
    public static void StatementOfWorkReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702491919);
        var items = new List<StatementOfWorkReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StatementOfWorkReservation.NewStatementOfWorkReservation($"statement-of-work-reservation-{index}", "sample"));
        }
        var report = StatementOfWorkReservationReport.BuildStatementOfWorkReservationReport("test", items, clock);
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
        StatementOfWorkReservationReportCountsEachRecordOnce();
    }
}
