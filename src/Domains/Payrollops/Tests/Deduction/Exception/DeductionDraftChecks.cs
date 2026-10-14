namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DeductionDraftChecks
{
    public static void DeductionDraftStartsInOpen()
    {
        var item = DeductionDraft.NewDeductionDraft("code-225", "title-422");
        Check.Equal(item.Status, DeductionDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionDraftMovesToReopened()
    {
        var item = DeductionDraft.NewDeductionDraft("code-225", "title-422");
        try
        {
            item.MoveTo(DeductionDraftStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionDraftStatus.Reopened, "status");
    }

    public static void DeductionDraftRejectsMovesFromArchived()
    {
        var item = DeductionDraft.NewDeductionDraft("code-225", "title-422");
        item.Status = DeductionDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(DeductionDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeductionDraftStartsInOpen();
        DeductionDraftMovesToReopened();
        DeductionDraftRejectsMovesFromArchived();
    }
}
