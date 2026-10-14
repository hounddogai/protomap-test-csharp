namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyWarrantyProposalReportChecks
{
    public static void LegacyWarrantyProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705329278);
        var items = new List<LegacyWarrantyProposal>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacyWarrantyProposal.NewLegacyWarrantyProposal($"legacy-warranty-proposal-{index}", "sample"));
        }
        var report = LegacyWarrantyProposalReport.BuildLegacyWarrantyProposalReport("test", items, clock);
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
        LegacyWarrantyProposalReportCountsEachRecordOnce();
    }
}
