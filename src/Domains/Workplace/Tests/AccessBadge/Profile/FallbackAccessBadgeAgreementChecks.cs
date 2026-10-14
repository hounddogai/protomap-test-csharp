namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class FallbackAccessBadgeAgreementChecks
{
    public static void FallbackAccessBadgeAgreementStartsInNew()
    {
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("code-852", "title-277");
        Check.Equal(item.Status, FallbackAccessBadgeAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAccessBadgeAgreementMovesToFailed()
    {
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("code-852", "title-277");
        try
        {
            item.MoveTo(FallbackAccessBadgeAgreementStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAccessBadgeAgreementStatus.Failed, "status");
    }

    public static void FallbackAccessBadgeAgreementRejectsMovesFromFailed()
    {
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("code-852", "title-277");
        item.Status = FallbackAccessBadgeAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackAccessBadgeAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeAgreementStartsInNew();
        FallbackAccessBadgeAgreementMovesToFailed();
        FallbackAccessBadgeAgreementRejectsMovesFromFailed();
    }
}
