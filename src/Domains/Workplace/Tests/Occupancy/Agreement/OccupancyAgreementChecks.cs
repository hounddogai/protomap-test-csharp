namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OccupancyAgreementChecks
{
    public static void OccupancyAgreementStartsInProposed()
    {
        var item = OccupancyAgreement.NewOccupancyAgreement("code-531", "title-400");
        Check.Equal(item.Status, OccupancyAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyAgreementMovesToRejected()
    {
        var item = OccupancyAgreement.NewOccupancyAgreement("code-531", "title-400");
        try
        {
            item.MoveTo(OccupancyAgreementStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyAgreementStatus.Rejected, "status");
    }

    public static void OccupancyAgreementRejectsMovesFromWithdrawn()
    {
        var item = OccupancyAgreement.NewOccupancyAgreement("code-531", "title-400");
        item.Status = OccupancyAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(OccupancyAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        OccupancyAgreementStartsInProposed();
        OccupancyAgreementMovesToRejected();
        OccupancyAgreementRejectsMovesFromWithdrawn();
    }
}
