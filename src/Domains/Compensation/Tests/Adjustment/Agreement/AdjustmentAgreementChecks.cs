namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentAgreementChecks
{
    public static void AdjustmentAgreementStartsInIdle()
    {
        var item = AdjustmentAgreement.NewAdjustmentAgreement("code-290", "name-80");
        Check.Equal(item.Status, AdjustmentAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentAgreementMovesToWarming()
    {
        var item = AdjustmentAgreement.NewAdjustmentAgreement("code-290", "name-80");
        try
        {
            item.MoveTo(AdjustmentAgreementStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentAgreementStatus.Warming, "status");
    }

    public static void AdjustmentAgreementRejectsMovesFromStopped()
    {
        var item = AdjustmentAgreement.NewAdjustmentAgreement("code-290", "name-80");
        item.Status = AdjustmentAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdjustmentAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdjustmentAgreementStartsInIdle();
        AdjustmentAgreementMovesToWarming();
        AdjustmentAgreementRejectsMovesFromStopped();
    }
}
