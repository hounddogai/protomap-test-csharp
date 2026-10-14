namespace Acme.Workforce.Contractors.Vendor.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor proposal.</summary>
public sealed record VendorProposalView(string CodeText, string Label, string CostCenter, string Capacity, string Grade, string DescriptionText, string StartsAtText, string VisibleText, string EmployeeId, string BatchSize, string StatusLabel)
{
    public static VendorProposalView ToVendorProposalView(VendorProposal item)
    {
        return new VendorProposalView(
            item.Code,
            item.Label,
            item.CostCenter,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            Text.Truncate(item.Description, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.EmployeeId,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorProposalView> ToVendorProposalViews(List<VendorProposal> items)
    {
        var views = new List<VendorProposalView>();
        foreach (var item in items)
        {
            views.Add(ToVendorProposalView(item));
        }
        return views;
    }

    public static double SumVendorProposalView(VendorProposalView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedStartsAt + (double)parsedBatchSize;
    }
}
