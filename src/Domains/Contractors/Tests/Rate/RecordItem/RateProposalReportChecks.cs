namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateProposalReportChecks
{
    public static void RateProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704596562);
        var items = new List<RateProposal>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RateProposal.NewRateProposal($"rate-proposal-{index}", "sample"));
        }
        var report = RateProposalReport.BuildRateProposalReport("test", items, clock);
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
        RateProposalReportCountsEachRecordOnce();
    }
}
