namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationMappingReportChecks
{
    public static void NominationMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701747405);
        var items = new List<NominationMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(NominationMapping.NewNominationMapping($"nomination-mapping-{index}", "sample"));
        }
        var report = NominationMappingReport.BuildNominationMappingReport("test", items, clock);
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
        NominationMappingReportCountsEachRecordOnce();
    }
}
