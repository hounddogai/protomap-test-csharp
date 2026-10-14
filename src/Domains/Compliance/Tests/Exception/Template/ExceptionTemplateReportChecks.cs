namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionTemplateReportChecks
{
    public static void ExceptionTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709726609);
        var items = new List<ExceptionTemplate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ExceptionTemplate.NewExceptionTemplate($"exception-template-{index}", "sample"));
        }
        var report = ExceptionTemplateReport.BuildExceptionTemplateReport("test", items, clock);
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
        ExceptionTemplateReportCountsEachRecordOnce();
    }
}
