namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyProposalChecks
{
    public static void WarrantyProposalStartsInPlanned()
    {
        var item = WarrantyProposal.NewWarrantyProposal("code-331", "title-473");
        Check.Equal(item.Status, WarrantyProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyProposalMovesToCancelled()
    {
        var item = WarrantyProposal.NewWarrantyProposal("code-331", "title-473");
        try
        {
            item.MoveTo(WarrantyProposalStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyProposalStatus.Cancelled, "status");
    }

    public static void WarrantyProposalRejectsMovesFromCancelled()
    {
        var item = WarrantyProposal.NewWarrantyProposal("code-331", "title-473");
        item.Status = WarrantyProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WarrantyProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WarrantyProposalStartsInPlanned();
        WarrantyProposalMovesToCancelled();
        WarrantyProposalRejectsMovesFromCancelled();
    }
}
