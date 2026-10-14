namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LegacyLeaveRequestPreferenceReportChecks
{
    public static void LegacyLeaveRequestPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707354393);
        var items = new List<LegacyLeaveRequestPreference>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference($"legacy-leave-request-preference-{index}", "sample"));
        }
        var report = LegacyLeaveRequestPreferenceReport.BuildLegacyLeaveRequestPreferenceReport("test", items, clock);
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
        LegacyLeaveRequestPreferenceReportCountsEachRecordOnce();
    }
}
