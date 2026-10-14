namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultAdjustmentAgreementChecks
{
    public static void DefaultAdjustmentAgreementStartsInCreated()
    {
        var item = DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement("code-9", "title-487");
        Check.Equal(item.Status, DefaultAdjustmentAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAdjustmentAgreementMovesToValidated()
    {
        var item = DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement("code-9", "title-487");
        try
        {
            item.MoveTo(DefaultAdjustmentAgreementStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAdjustmentAgreementStatus.Validated, "status");
    }

    public static void DefaultAdjustmentAgreementRejectsMovesFromRetired()
    {
        var item = DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement("code-9", "title-487");
        item.Status = DefaultAdjustmentAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultAdjustmentAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultAdjustmentAgreementStartsInCreated();
        DefaultAdjustmentAgreementMovesToValidated();
        DefaultAdjustmentAgreementRejectsMovesFromRetired();
    }
}
