namespace Acme.Workforce.Payrollops.Tests.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Checks of legacy garnishment agreement code, run by the project's runner.</summary>
public static class RegionalGarnishmentAgreementChecks
{
    public static void RegionalGarnishmentAgreementStartsInProposed()
    {
        var item = RegionalGarnishmentAgreement.NewRegionalGarnishmentAgreement("code-520", "label-468");
        Check.Equal(item.Status, RegionalGarnishmentAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalGarnishmentAgreementMovesToApproved()
    {
        var item = RegionalGarnishmentAgreement.NewRegionalGarnishmentAgreement("code-520", "label-468");
        try
        {
            item.MoveTo(RegionalGarnishmentAgreementStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalGarnishmentAgreementStatus.Approved, "status");
    }

    public static void RegionalGarnishmentAgreementRejectsMovesFromWithdrawn()
    {
        var item = RegionalGarnishmentAgreement.NewRegionalGarnishmentAgreement("code-520", "label-468");
        item.Status = RegionalGarnishmentAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalGarnishmentAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalGarnishmentAgreementStartsInProposed();
        RegionalGarnishmentAgreementMovesToApproved();
        RegionalGarnishmentAgreementRejectsMovesFromWithdrawn();
    }
}
