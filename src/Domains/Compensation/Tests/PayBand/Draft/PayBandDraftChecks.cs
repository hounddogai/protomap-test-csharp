namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandDraftChecks
{
    public static void PayBandDraftStartsInProposed()
    {
        var item = PayBandDraft.NewPayBandDraft("code-598", "name-624");
        Check.Equal(item.Status, PayBandDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandDraftMovesToApproved()
    {
        var item = PayBandDraft.NewPayBandDraft("code-598", "name-624");
        try
        {
            item.MoveTo(PayBandDraftStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandDraftStatus.Approved, "status");
    }

    public static void PayBandDraftRejectsMovesFromWithdrawn()
    {
        var item = PayBandDraft.NewPayBandDraft("code-598", "name-624");
        item.Status = PayBandDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayBandDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayBandDraftStartsInProposed();
        PayBandDraftMovesToApproved();
        PayBandDraftRejectsMovesFromWithdrawn();
    }
}
