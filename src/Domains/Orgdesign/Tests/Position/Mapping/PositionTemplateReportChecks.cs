namespace Acme.Workforce.Orgdesign.Tests.Position.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PositionTemplateReportChecks
{
    public static void PositionTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702555412);
        var items = new List<PositionTemplate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PositionTemplate.NewPositionTemplate($"position-template-{index}", "sample"));
        }
        var report = PositionTemplateReport.BuildPositionTemplateReport("test", items, clock);
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
        PositionTemplateReportCountsEachRecordOnce();
    }
}
