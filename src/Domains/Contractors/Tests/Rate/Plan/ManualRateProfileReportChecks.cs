namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ManualRateProfileReportChecks
{
    public static void ManualRateProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709643646);
        var items = new List<ManualRateProfile>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualRateProfile.NewManualRateProfile($"manual-rate-profile-{index}", "sample"));
        }
        var report = ManualRateProfileReport.BuildManualRateProfileReport("test", items, clock);
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
        ManualRateProfileReportCountsEachRecordOnce();
    }
}
