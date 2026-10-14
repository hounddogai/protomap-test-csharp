namespace Acme.Workforce.Contractors.Vendor.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor preference.</summary>
public sealed record VendorPreferenceView(string Code, string Label, string Sequence, string UpdatedAt, string Amount, string FloorValue, string Maximum, string Region, string ReviewerText, string StatusLabel)
{
    public static VendorPreferenceView ToVendorPreferenceView(VendorPreference item)
    {
        return new VendorPreferenceView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 64),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 40),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorPreferenceView> ToVendorPreferenceViews(List<VendorPreference> items)
    {
        var views = new List<VendorPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToVendorPreferenceView(item));
        }
        return views;
    }

    public static double SumVendorPreferenceView(VendorPreferenceView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedUpdatedAt + parsedAmount;
    }
}
