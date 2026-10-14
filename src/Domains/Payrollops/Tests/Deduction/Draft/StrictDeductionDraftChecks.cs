namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictDeductionDraftChecks
{
    public static void StrictDeductionDraftStartsInRequested()
    {
        var item = StrictDeductionDraft.NewStrictDeductionDraft("code-671", "title-477");
        Check.Equal(item.Status, StrictDeductionDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictDeductionDraftMovesToRevoked()
    {
        var item = StrictDeductionDraft.NewStrictDeductionDraft("code-671", "title-477");
        try
        {
            item.MoveTo(StrictDeductionDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictDeductionDraftStatus.Revoked, "status");
    }

    public static void StrictDeductionDraftRejectsMovesFromExpired()
    {
        var item = StrictDeductionDraft.NewStrictDeductionDraft("code-671", "title-477");
        item.Status = StrictDeductionDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictDeductionDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictDeductionDraftStartsInRequested();
        StrictDeductionDraftMovesToRevoked();
        StrictDeductionDraftRejectsMovesFromExpired();
    }
}
