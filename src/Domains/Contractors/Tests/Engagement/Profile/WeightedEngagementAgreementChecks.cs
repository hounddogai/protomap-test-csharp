namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedEngagementAgreementChecks
{
    public static void WeightedEngagementAgreementStartsInDraft()
    {
        var item = WeightedEngagementAgreement.NewWeightedEngagementAgreement("code-552", "name-931");
        Check.Equal(item.Status, WeightedEngagementAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedEngagementAgreementMovesToPending()
    {
        var item = WeightedEngagementAgreement.NewWeightedEngagementAgreement("code-552", "name-931");
        try
        {
            item.MoveTo(WeightedEngagementAgreementStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedEngagementAgreementStatus.Pending, "status");
    }

    public static void WeightedEngagementAgreementRejectsMovesFromClosed()
    {
        var item = WeightedEngagementAgreement.NewWeightedEngagementAgreement("code-552", "name-931");
        item.Status = WeightedEngagementAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedEngagementAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedEngagementAgreementStartsInDraft();
        WeightedEngagementAgreementMovesToPending();
        WeightedEngagementAgreementRejectsMovesFromClosed();
    }
}
