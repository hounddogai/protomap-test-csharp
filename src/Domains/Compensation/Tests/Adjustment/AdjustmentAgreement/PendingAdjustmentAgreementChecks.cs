namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class PendingAdjustmentAgreementChecks
{
    public static void PendingAdjustmentAgreementStartsInNew()
    {
        var item = PendingAdjustmentAgreement.NewPendingAdjustmentAgreement("code-376", "title-670");
        Check.Equal(item.Status, PendingAdjustmentAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAdjustmentAgreementMovesToDone()
    {
        var item = PendingAdjustmentAgreement.NewPendingAdjustmentAgreement("code-376", "title-670");
        try
        {
            item.MoveTo(PendingAdjustmentAgreementStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAdjustmentAgreementStatus.Done, "status");
    }

    public static void PendingAdjustmentAgreementRejectsMovesFromFailed()
    {
        var item = PendingAdjustmentAgreement.NewPendingAdjustmentAgreement("code-376", "title-670");
        item.Status = PendingAdjustmentAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingAdjustmentAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingAdjustmentAgreementStartsInNew();
        PendingAdjustmentAgreementMovesToDone();
        PendingAdjustmentAgreementRejectsMovesFromFailed();
    }
}
