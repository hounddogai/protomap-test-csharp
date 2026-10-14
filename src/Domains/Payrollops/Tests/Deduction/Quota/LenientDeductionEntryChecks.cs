namespace Acme.Workforce.Payrollops.Tests.Deduction.Quota;

using Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LenientDeductionEntryChecks
{
    public static void LenientDeductionEntryStartsInOpen()
    {
        var item = LenientDeductionEntry.NewLenientDeductionEntry("code-649", "title-982");
        Check.Equal(item.Status, LenientDeductionEntryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientDeductionEntryMovesToInReview()
    {
        var item = LenientDeductionEntry.NewLenientDeductionEntry("code-649", "title-982");
        try
        {
            item.MoveTo(LenientDeductionEntryStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientDeductionEntryStatus.InReview, "status");
    }

    public static void LenientDeductionEntryRejectsMovesFromArchived()
    {
        var item = LenientDeductionEntry.NewLenientDeductionEntry("code-649", "title-982");
        item.Status = LenientDeductionEntryStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientDeductionEntryStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientDeductionEntryStartsInOpen();
        LenientDeductionEntryMovesToInReview();
        LenientDeductionEntryRejectsMovesFromArchived();
    }
}
