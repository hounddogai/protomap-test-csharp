namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RegionalWarrantyProposalChecks
{
    public static void RegionalWarrantyProposalStartsInCreated()
    {
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("code-682", "name-99");
        Check.Equal(item.Status, RegionalWarrantyProposalStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalWarrantyProposalMovesToValidated()
    {
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("code-682", "name-99");
        try
        {
            item.MoveTo(RegionalWarrantyProposalStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalWarrantyProposalStatus.Validated, "status");
    }

    public static void RegionalWarrantyProposalRejectsMovesFromRetired()
    {
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("code-682", "name-99");
        item.Status = RegionalWarrantyProposalStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalWarrantyProposalStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalWarrantyProposalStartsInCreated();
        RegionalWarrantyProposalMovesToValidated();
        RegionalWarrantyProposalRejectsMovesFromRetired();
    }
}
