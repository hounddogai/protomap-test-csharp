namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyAgreementChecks
{
    public static void WarrantyAgreementStartsInIdle()
    {
        var item = WarrantyAgreement.NewWarrantyAgreement("code-917", "title-106");
        Check.Equal(item.Status, WarrantyAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyAgreementMovesToWarming()
    {
        var item = WarrantyAgreement.NewWarrantyAgreement("code-917", "title-106");
        try
        {
            item.MoveTo(WarrantyAgreementStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyAgreementStatus.Warming, "status");
    }

    public static void WarrantyAgreementRejectsMovesFromStopped()
    {
        var item = WarrantyAgreement.NewWarrantyAgreement("code-917", "title-106");
        item.Status = WarrantyAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(WarrantyAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WarrantyAgreementStartsInIdle();
        WarrantyAgreementMovesToWarming();
        WarrantyAgreementRejectsMovesFromStopped();
    }
}
