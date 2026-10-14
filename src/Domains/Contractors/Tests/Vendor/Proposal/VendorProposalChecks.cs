namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class VendorProposalChecks
{
    public static void VendorProposalStartsInDraft()
    {
        var item = VendorProposal.NewVendorProposal("code-790", "label-994");
        Check.Equal(item.Status, VendorProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorProposalMovesToPending()
    {
        var item = VendorProposal.NewVendorProposal("code-790", "label-994");
        try
        {
            item.MoveTo(VendorProposalStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorProposalStatus.Pending, "status");
    }

    public static void VendorProposalRejectsMovesFromClosed()
    {
        var item = VendorProposal.NewVendorProposal("code-790", "label-994");
        item.Status = VendorProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(VendorProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VendorProposalStartsInDraft();
        VendorProposalMovesToPending();
        VendorProposalRejectsMovesFromClosed();
    }
}
