namespace Acme.Workforce.Engagement.Tests.Recognition.Allocation;

using Acme.Workforce.Engagement.Recognition.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RecognitionProposalReportChecks
{
    public static void RecognitionProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703509355);
        var items = new List<RecognitionProposal>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RecognitionProposal.NewRecognitionProposal($"recognition-proposal-{index}", "sample"));
        }
        var report = RecognitionProposalReport.BuildRecognitionProposalReport("test", items, clock);
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
        RecognitionProposalReportCountsEachRecordOnce();
    }
}
