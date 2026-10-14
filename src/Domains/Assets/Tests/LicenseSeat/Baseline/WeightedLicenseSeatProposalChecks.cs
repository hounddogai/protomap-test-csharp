namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedLicenseSeatProposalChecks
{
    public static void WeightedLicenseSeatProposalStartsInOpen()
    {
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("code-782", "name-601");
        Check.Equal(item.Status, WeightedLicenseSeatProposalStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedLicenseSeatProposalMovesToInReview()
    {
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("code-782", "name-601");
        try
        {
            item.MoveTo(WeightedLicenseSeatProposalStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedLicenseSeatProposalStatus.InReview, "status");
    }

    public static void WeightedLicenseSeatProposalRejectsMovesFromArchived()
    {
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("code-782", "name-601");
        item.Status = WeightedLicenseSeatProposalStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedLicenseSeatProposalStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatProposalStartsInOpen();
        WeightedLicenseSeatProposalMovesToInReview();
        WeightedLicenseSeatProposalRejectsMovesFromArchived();
    }
}
