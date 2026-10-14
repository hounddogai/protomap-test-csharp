namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyWarrantyProposalViewChecks
{
    public static void LegacyWarrantyProposalViewShowsTheStatusInCapitals()
    {
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("code-988", "title-937");
        var view = LegacyWarrantyProposalView.ToLegacyWarrantyProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyWarrantyProposalView.ToLegacyWarrantyProposalViews(new List<LegacyWarrantyProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyWarrantyProposalViewShowsTheStatusInCapitals();
    }
}
