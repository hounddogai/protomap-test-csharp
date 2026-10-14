namespace Acme.Workforce.Assets.Warranty.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy warranty proposal.</summary>
public sealed record LegacyWarrantyProposalView(string Code, string Title, string Description, string Team, string Capacity, string Currency, string Ratio, string NameText, string StatusLabel)
{
    public static LegacyWarrantyProposalView ToLegacyWarrantyProposalView(LegacyWarrantyProposal item)
    {
        return new LegacyWarrantyProposalView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Description, 120),
            item.Team,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyWarrantyProposalView> ToLegacyWarrantyProposalViews(List<LegacyWarrantyProposal> items)
    {
        var views = new List<LegacyWarrantyProposalView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyWarrantyProposalView(item));
        }
        return views;
    }

    public static double SumLegacyWarrantyProposalView(LegacyWarrantyProposalView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedRatio;
    }
}
