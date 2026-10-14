namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AccessBadgeAgreementChecks
{
    public static void AccessBadgeAgreementStartsInCreated()
    {
        var item = AccessBadgeAgreement.NewAccessBadgeAgreement("code-924", "name-890");
        Check.Equal(item.Status, AccessBadgeAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeAgreementMovesToPublished()
    {
        var item = AccessBadgeAgreement.NewAccessBadgeAgreement("code-924", "name-890");
        try
        {
            item.MoveTo(AccessBadgeAgreementStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeAgreementStatus.Published, "status");
    }

    public static void AccessBadgeAgreementRejectsMovesFromRetired()
    {
        var item = AccessBadgeAgreement.NewAccessBadgeAgreement("code-924", "name-890");
        item.Status = AccessBadgeAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(AccessBadgeAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        AccessBadgeAgreementStartsInCreated();
        AccessBadgeAgreementMovesToPublished();
        AccessBadgeAgreementRejectsMovesFromRetired();
    }
}
