namespace Acme.Workforce.Payrollops.Tests.Garnishment.Agreement;

using Acme.Workforce.Payrollops.Garnishment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GarnishmentAgreementChecks
{
    public static void GarnishmentAgreementStartsInCreated()
    {
        var item = GarnishmentAgreement.NewGarnishmentAgreement("code-840", "title-387");
        Check.Equal(item.Status, GarnishmentAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentAgreementMovesToRetired()
    {
        var item = GarnishmentAgreement.NewGarnishmentAgreement("code-840", "title-387");
        try
        {
            item.MoveTo(GarnishmentAgreementStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentAgreementStatus.Retired, "status");
    }

    public static void GarnishmentAgreementRejectsMovesFromRetired()
    {
        var item = GarnishmentAgreement.NewGarnishmentAgreement("code-840", "title-387");
        item.Status = GarnishmentAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(GarnishmentAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        GarnishmentAgreementStartsInCreated();
        GarnishmentAgreementMovesToRetired();
        GarnishmentAgreementRejectsMovesFromRetired();
    }
}
