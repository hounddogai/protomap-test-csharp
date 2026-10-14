namespace Acme.Workforce.Assets.Tests.LicenseSeat.Proposal;

using Acme.Workforce.Assets.LicenseSeat.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LicenseSeatProposalChecks
{
    public static void LicenseSeatProposalStartsInRequested()
    {
        var item = LicenseSeatProposal.NewLicenseSeatProposal("code-254", "label-354");
        Check.Equal(item.Status, LicenseSeatProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatProposalMovesToExpired()
    {
        var item = LicenseSeatProposal.NewLicenseSeatProposal("code-254", "label-354");
        try
        {
            item.MoveTo(LicenseSeatProposalStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatProposalStatus.Expired, "status");
    }

    public static void LicenseSeatProposalRejectsMovesFromExpired()
    {
        var item = LicenseSeatProposal.NewLicenseSeatProposal("code-254", "label-354");
        item.Status = LicenseSeatProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(LicenseSeatProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LicenseSeatProposalStartsInRequested();
        LicenseSeatProposalMovesToExpired();
        LicenseSeatProposalRejectsMovesFromExpired();
    }
}
