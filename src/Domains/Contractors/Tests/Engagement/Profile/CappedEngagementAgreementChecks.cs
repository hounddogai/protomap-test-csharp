namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedEngagementAgreementChecks
{
    public static void CappedEngagementAgreementStartsInNew()
    {
        var item = CappedEngagementAgreement.NewCappedEngagementAgreement("code-439", "name-342");
        Check.Equal(item.Status, CappedEngagementAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedEngagementAgreementMovesToFailed()
    {
        var item = CappedEngagementAgreement.NewCappedEngagementAgreement("code-439", "name-342");
        try
        {
            item.MoveTo(CappedEngagementAgreementStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedEngagementAgreementStatus.Failed, "status");
    }

    public static void CappedEngagementAgreementRejectsMovesFromFailed()
    {
        var item = CappedEngagementAgreement.NewCappedEngagementAgreement("code-439", "name-342");
        item.Status = CappedEngagementAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedEngagementAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedEngagementAgreementStartsInNew();
        CappedEngagementAgreementMovesToFailed();
        CappedEngagementAgreementRejectsMovesFromFailed();
    }
}
