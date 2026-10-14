namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredAssignmentAgreementChecks
{
    public static void TieredAssignmentAgreementStartsInProposed()
    {
        var item = TieredAssignmentAgreement.NewTieredAssignmentAgreement("code-132", "label-660");
        Check.Equal(item.Status, TieredAssignmentAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAssignmentAgreementMovesToRejected()
    {
        var item = TieredAssignmentAgreement.NewTieredAssignmentAgreement("code-132", "label-660");
        try
        {
            item.MoveTo(TieredAssignmentAgreementStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAssignmentAgreementStatus.Rejected, "status");
    }

    public static void TieredAssignmentAgreementRejectsMovesFromWithdrawn()
    {
        var item = TieredAssignmentAgreement.NewTieredAssignmentAgreement("code-132", "label-660");
        item.Status = TieredAssignmentAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredAssignmentAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredAssignmentAgreementStartsInProposed();
        TieredAssignmentAgreementMovesToRejected();
        TieredAssignmentAgreementRejectsMovesFromWithdrawn();
    }
}
