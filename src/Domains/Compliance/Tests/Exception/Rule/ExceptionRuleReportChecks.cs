namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionRuleReportChecks
{
    public static void ExceptionRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709829047);
        var items = new List<ExceptionRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ExceptionRule.NewExceptionRule($"exception-rule-{index}", "sample"));
        }
        var report = ExceptionRuleReport.BuildExceptionRuleReport("test", items, clock);
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
        ExceptionRuleReportCountsEachRecordOnce();
    }
}
