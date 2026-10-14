namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyWarrantyProposalChecks
{
    public static void LegacyWarrantyProposalStartsInRequested()
    {
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("code-988", "title-937");
        Check.Equal(item.Status, LegacyWarrantyProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyWarrantyProposalMovesToGranted()
    {
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("code-988", "title-937");
        try
        {
            item.MoveTo(LegacyWarrantyProposalStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyWarrantyProposalStatus.Granted, "status");
    }

    public static void LegacyWarrantyProposalRejectsMovesFromExpired()
    {
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("code-988", "title-937");
        item.Status = LegacyWarrantyProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(LegacyWarrantyProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LegacyWarrantyProposalStartsInRequested();
        LegacyWarrantyProposalMovesToGranted();
        LegacyWarrantyProposalRejectsMovesFromExpired();
    }
}
