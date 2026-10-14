namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DeductionSegmentChecks
{
    public static void DeductionSegmentStartsInRequested()
    {
        var item = DeductionSegment.NewDeductionSegment("code-465", "name-85");
        Check.Equal(item.Status, DeductionSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionSegmentMovesToGranted()
    {
        var item = DeductionSegment.NewDeductionSegment("code-465", "name-85");
        try
        {
            item.MoveTo(DeductionSegmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionSegmentStatus.Granted, "status");
    }

    public static void DeductionSegmentRejectsMovesFromExpired()
    {
        var item = DeductionSegment.NewDeductionSegment("code-465", "name-85");
        item.Status = DeductionSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(DeductionSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DeductionSegmentStartsInRequested();
        DeductionSegmentMovesToGranted();
        DeductionSegmentRejectsMovesFromExpired();
    }
}
