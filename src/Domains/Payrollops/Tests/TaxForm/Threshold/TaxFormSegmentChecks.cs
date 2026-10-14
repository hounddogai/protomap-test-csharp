namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TaxFormSegmentChecks
{
    public static void TaxFormSegmentStartsInRequested()
    {
        var item = TaxFormSegment.NewTaxFormSegment("code-115", "name-144");
        Check.Equal(item.Status, TaxFormSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormSegmentMovesToGranted()
    {
        var item = TaxFormSegment.NewTaxFormSegment("code-115", "name-144");
        try
        {
            item.MoveTo(TaxFormSegmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormSegmentStatus.Granted, "status");
    }

    public static void TaxFormSegmentRejectsMovesFromExpired()
    {
        var item = TaxFormSegment.NewTaxFormSegment("code-115", "name-144");
        item.Status = TaxFormSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(TaxFormSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TaxFormSegmentStartsInRequested();
        TaxFormSegmentMovesToGranted();
        TaxFormSegmentRejectsMovesFromExpired();
    }
}
