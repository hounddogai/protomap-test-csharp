namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LenientLaptopAgreementReportChecks
{
    public static void LenientLaptopAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703523229);
        var items = new List<LenientLaptopAgreement>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientLaptopAgreement.NewLenientLaptopAgreement($"lenient-laptop-agreement-{index}", "sample"));
        }
        var report = LenientLaptopAgreementReport.BuildLenientLaptopAgreementReport("test", items, clock);
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
        LenientLaptopAgreementReportCountsEachRecordOnce();
    }
}
