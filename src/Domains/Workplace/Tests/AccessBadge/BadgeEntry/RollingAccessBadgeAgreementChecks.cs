namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class RollingAccessBadgeAgreementChecks
{
    public static void RollingAccessBadgeAgreementStartsInCreated()
    {
        var item = RollingAccessBadgeAgreement.NewRollingAccessBadgeAgreement("code-158", "title-726");
        Check.Equal(item.Status, RollingAccessBadgeAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAccessBadgeAgreementMovesToPublished()
    {
        var item = RollingAccessBadgeAgreement.NewRollingAccessBadgeAgreement("code-158", "title-726");
        try
        {
            item.MoveTo(RollingAccessBadgeAgreementStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAccessBadgeAgreementStatus.Published, "status");
    }

    public static void RollingAccessBadgeAgreementRejectsMovesFromRetired()
    {
        var item = RollingAccessBadgeAgreement.NewRollingAccessBadgeAgreement("code-158", "title-726");
        item.Status = RollingAccessBadgeAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(RollingAccessBadgeAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        RollingAccessBadgeAgreementStartsInCreated();
        RollingAccessBadgeAgreementMovesToPublished();
        RollingAccessBadgeAgreementRejectsMovesFromRetired();
    }
}
