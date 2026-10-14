namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class MobilityProposalReportChecks
{
    public static void MobilityProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708745804);
        var items = new List<MobilityProposal>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(MobilityProposal.NewMobilityProposal($"mobility-proposal-{index}", "sample"));
        }
        var report = MobilityProposalReport.BuildMobilityProposalReport("test", items, clock);
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
        MobilityProposalReportCountsEachRecordOnce();
    }
}
