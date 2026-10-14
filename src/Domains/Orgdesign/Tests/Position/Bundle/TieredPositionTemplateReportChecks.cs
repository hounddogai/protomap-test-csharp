namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredPositionTemplateReportChecks
{
    public static void TieredPositionTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703440768);
        var items = new List<TieredPositionTemplate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredPositionTemplate.NewTieredPositionTemplate($"tiered-position-template-{index}", "sample"));
        }
        var report = TieredPositionTemplateReport.BuildTieredPositionTemplateReport("test", items, clock);
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
        TieredPositionTemplateReportCountsEachRecordOnce();
    }
}
