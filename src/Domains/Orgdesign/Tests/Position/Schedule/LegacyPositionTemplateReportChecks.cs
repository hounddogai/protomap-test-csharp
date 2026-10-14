namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LegacyPositionTemplateReportChecks
{
    public static void LegacyPositionTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701475648);
        var items = new List<LegacyPositionTemplate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacyPositionTemplate.NewLegacyPositionTemplate($"legacy-position-template-{index}", "sample"));
        }
        var report = LegacyPositionTemplateReport.BuildLegacyPositionTemplateReport("test", items, clock);
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
        LegacyPositionTemplateReportCountsEachRecordOnce();
    }
}
