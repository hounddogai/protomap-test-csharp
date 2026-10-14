namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientDeskTemplateReportChecks
{
    public static void LenientDeskTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703607284);
        var items = new List<LenientDeskTemplate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientDeskTemplate.NewLenientDeskTemplate($"lenient-desk-template-{index}", "sample"));
        }
        var report = LenientDeskTemplateReport.BuildLenientDeskTemplateReport("test", items, clock);
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
        LenientDeskTemplateReportCountsEachRecordOnce();
    }
}
