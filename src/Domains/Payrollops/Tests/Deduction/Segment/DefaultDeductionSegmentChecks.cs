namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultDeductionSegmentChecks
{
    public static void DefaultDeductionSegmentStartsInPlanned()
    {
        var item = DefaultDeductionSegment.NewDefaultDeductionSegment("code-175", "label-175");
        Check.Equal(item.Status, DefaultDeductionSegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultDeductionSegmentMovesToInProgress()
    {
        var item = DefaultDeductionSegment.NewDefaultDeductionSegment("code-175", "label-175");
        try
        {
            item.MoveTo(DefaultDeductionSegmentStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultDeductionSegmentStatus.InProgress, "status");
    }

    public static void DefaultDeductionSegmentRejectsMovesFromCancelled()
    {
        var item = DefaultDeductionSegment.NewDefaultDeductionSegment("code-175", "label-175");
        item.Status = DefaultDeductionSegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultDeductionSegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultDeductionSegmentStartsInPlanned();
        DefaultDeductionSegmentMovesToInProgress();
        DefaultDeductionSegmentRejectsMovesFromCancelled();
    }
}
