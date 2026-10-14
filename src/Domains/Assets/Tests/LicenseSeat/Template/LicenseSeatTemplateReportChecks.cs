namespace Acme.Workforce.Assets.Tests.LicenseSeat.Template;

using Acme.Workforce.Assets.LicenseSeat.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LicenseSeatTemplateReportChecks
{
    public static void LicenseSeatTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701800293);
        var items = new List<LicenseSeatTemplate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LicenseSeatTemplate.NewLicenseSeatTemplate($"license-seat-template-{index}", "sample"));
        }
        var report = LicenseSeatTemplateReport.BuildLicenseSeatTemplateReport("test", items, clock);
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
        LicenseSeatTemplateReportCountsEachRecordOnce();
    }
}
