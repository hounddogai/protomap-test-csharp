namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StrictWarrantyDraftReportChecks
{
    public static void StrictWarrantyDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700761416);
        var items = new List<StrictWarrantyDraft>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictWarrantyDraft.NewStrictWarrantyDraft($"strict-warranty-draft-{index}", "sample"));
        }
        var report = StrictWarrantyDraftReport.BuildStrictWarrantyDraftReport("test", items, clock);
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
        StrictWarrantyDraftReportCountsEachRecordOnce();
    }
}
