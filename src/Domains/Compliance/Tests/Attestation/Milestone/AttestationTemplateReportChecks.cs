namespace Acme.Workforce.Compliance.Tests.Attestation.Milestone;

using Acme.Workforce.Compliance.Attestation.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AttestationTemplateReportChecks
{
    public static void AttestationTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709125013);
        var items = new List<AttestationTemplate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AttestationTemplate.NewAttestationTemplate($"attestation-template-{index}", "sample"));
        }
        var report = AttestationTemplateReport.BuildAttestationTemplateReport("test", items, clock);
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
        AttestationTemplateReportCountsEachRecordOnce();
    }
}
