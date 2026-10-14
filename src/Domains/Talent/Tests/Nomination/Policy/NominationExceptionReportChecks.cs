namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationExceptionReportChecks
{
    public static void NominationExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703974541);
        var items = new List<NominationException>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(NominationException.NewNominationException($"nomination-exception-{index}", "sample"));
        }
        var report = NominationExceptionReport.BuildNominationExceptionReport("test", items, clock);
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
        NominationExceptionReportCountsEachRecordOnce();
    }
}
