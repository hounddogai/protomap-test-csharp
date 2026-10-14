namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PrimaryDeductionProfileReportChecks
{
    public static void PrimaryDeductionProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704443787);
        var items = new List<PrimaryDeductionProfile>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryDeductionProfile.NewPrimaryDeductionProfile($"primary-deduction-profile-{index}", "sample"));
        }
        var report = PrimaryDeductionProfileReport.BuildPrimaryDeductionProfileReport("test", items, clock);
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
        PrimaryDeductionProfileReportCountsEachRecordOnce();
    }
}
