namespace Acme.Workforce.Assets.Warranty.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional warranty proposal.</summary>
public sealed record RegionalWarrantyProposalView(string Code, string Name, string EmployeeIdText, string Source, string Category, string CreatedAtText, string BadgeText, string RatingText, string StatusLabel)
{
    public static RegionalWarrantyProposalView ToRegionalWarrantyProposalView(RegionalWarrantyProposal item)
    {
        return new RegionalWarrantyProposalView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.EmployeeId,
            item.Source,
            Text.Truncate(item.Category, 80),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalWarrantyProposalView> ToRegionalWarrantyProposalViews(List<RegionalWarrantyProposal> items)
    {
        var views = new List<RegionalWarrantyProposalView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalWarrantyProposalView(item));
        }
        return views;
    }

    public static double SumRegionalWarrantyProposalView(RegionalWarrantyProposalView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedRating;
    }
}
