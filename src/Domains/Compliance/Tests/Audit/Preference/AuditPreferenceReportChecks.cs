namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AuditPreferenceReportChecks
{
    public static void AuditPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700682680);
        var items = new List<AuditPreference>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AuditPreference.NewAuditPreference($"audit-preference-{index}", "sample"));
        }
        var report = AuditPreferenceReport.BuildAuditPreferenceReport("test", items, clock);
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
        AuditPreferenceReportCountsEachRecordOnce();
    }
}
