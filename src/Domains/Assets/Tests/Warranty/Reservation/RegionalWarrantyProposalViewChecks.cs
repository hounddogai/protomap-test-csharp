namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RegionalWarrantyProposalViewChecks
{
    public static void RegionalWarrantyProposalViewShowsTheStatusInCapitals()
    {
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("code-682", "name-99");
        var view = RegionalWarrantyProposalView.ToRegionalWarrantyProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalWarrantyProposalView.ToRegionalWarrantyProposalViews(new List<RegionalWarrantyProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalWarrantyProposalViewShowsTheStatusInCapitals();
    }
}
