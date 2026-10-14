namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAgreement;

using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Checks of occupancy agreement code, run by the project's runner.</summary>
public static class AdaptiveOccupancyAgreementChecks
{
    public static void AdaptiveOccupancyAgreementStartsInNew()
    {
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("code-37", "name-725");
        Check.Equal(item.Status, AdaptiveOccupancyAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveOccupancyAgreementMovesToQueued()
    {
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("code-37", "name-725");
        try
        {
            item.MoveTo(AdaptiveOccupancyAgreementStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveOccupancyAgreementStatus.Queued, "status");
    }

    public static void AdaptiveOccupancyAgreementRejectsMovesFromFailed()
    {
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("code-37", "name-725");
        item.Status = AdaptiveOccupancyAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveOccupancyAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveOccupancyAgreementStartsInNew();
        AdaptiveOccupancyAgreementMovesToQueued();
        AdaptiveOccupancyAgreementRejectsMovesFromFailed();
    }
}
