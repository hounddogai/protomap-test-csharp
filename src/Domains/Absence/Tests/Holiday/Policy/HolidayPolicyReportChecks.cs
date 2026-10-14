namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayPolicyReportChecks
{
    public static void HolidayPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705270176);
        var items = new List<HolidayPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HolidayPolicy.NewHolidayPolicy($"holiday-policy-{index}", "sample"));
        }
        var report = HolidayPolicyReport.BuildHolidayPolicyReport("test", items, clock);
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
        HolidayPolicyReportCountsEachRecordOnce();
    }
}
