namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class TieredEngagementAgreementChecks
{
    public static void TieredEngagementAgreementStartsInCreated()
    {
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("code-279", "title-646");
        Check.Equal(item.Status, TieredEngagementAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredEngagementAgreementMovesToRetired()
    {
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("code-279", "title-646");
        try
        {
            item.MoveTo(TieredEngagementAgreementStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredEngagementAgreementStatus.Retired, "status");
    }

    public static void TieredEngagementAgreementRejectsMovesFromRetired()
    {
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("code-279", "title-646");
        item.Status = TieredEngagementAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredEngagementAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredEngagementAgreementStartsInCreated();
        TieredEngagementAgreementMovesToRetired();
        TieredEngagementAgreementRejectsMovesFromRetired();
    }
}
