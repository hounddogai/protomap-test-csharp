namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class VendorProposalViewChecks
{
    public static void VendorProposalViewShowsTheStatusInCapitals()
    {
        var item = VendorProposal.NewVendorProposal("code-790", "label-994");
        var view = VendorProposalView.ToVendorProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorProposalView.ToVendorProposalViews(new List<VendorProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorProposalViewShowsTheStatusInCapitals();
    }
}
