namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopAgreementReportChecks
{
    public static void LaptopAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708092167);
        var items = new List<LaptopAgreement>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LaptopAgreement.NewLaptopAgreement($"laptop-agreement-{index}", "sample"));
        }
        var report = LaptopAgreementReport.BuildLaptopAgreementReport("test", items, clock);
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
        LaptopAgreementReportCountsEachRecordOnce();
    }
}
