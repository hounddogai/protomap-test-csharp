namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionSegmentChecks
{
    public static void StrictDeductionSegmentStartsInNew()
    {
        var item = StrictDeductionSegment.NewStrictDeductionSegment("code-200", "name-78");
        Check.Equal(item.Status, StrictDeductionSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictDeductionSegmentMovesToDone()
    {
        var item = StrictDeductionSegment.NewStrictDeductionSegment("code-200", "name-78");
        try
        {
            item.MoveTo(StrictDeductionSegmentStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictDeductionSegmentStatus.Done, "status");
    }

    public static void StrictDeductionSegmentRejectsMovesFromFailed()
    {
        var item = StrictDeductionSegment.NewStrictDeductionSegment("code-200", "name-78");
        item.Status = StrictDeductionSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictDeductionSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictDeductionSegmentStartsInNew();
        StrictDeductionSegmentMovesToDone();
        StrictDeductionSegmentRejectsMovesFromFailed();
    }
}
