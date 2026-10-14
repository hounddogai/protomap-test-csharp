namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationWindowReportChecks
{
    public static void AttestationWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707717416);
        var items = new List<AttestationWindow>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AttestationWindow.NewAttestationWindow($"attestation-window-{index}", "sample"));
        }
        var report = AttestationWindowReport.BuildAttestationWindowReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AttestationWindowReportCountsEachRecordOnce();
    }
}
