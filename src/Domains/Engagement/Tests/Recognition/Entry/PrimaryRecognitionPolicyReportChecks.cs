namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PrimaryRecognitionPolicyReportChecks
{
    public static void PrimaryRecognitionPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701894054);
        var items = new List<PrimaryRecognitionPolicy>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy($"primary-recognition-policy-{index}", "sample"));
        }
        var report = PrimaryRecognitionPolicyReport.BuildPrimaryRecognitionPolicyReport("test", items, clock);
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
        PrimaryRecognitionPolicyReportCountsEachRecordOnce();
    }
}
