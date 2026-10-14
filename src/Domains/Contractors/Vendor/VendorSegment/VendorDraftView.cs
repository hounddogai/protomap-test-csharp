namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor draft.</summary>
public sealed record VendorDraftView(string Code, string Title, string ArchivedText, string AttemptsText, string Version, string Maximum, string Grade, string Ratio, string BatchSize, string CountryText, string StatusLabel)
{
    public static VendorDraftView ToVendorDraftView(VendorDraft item)
    {
        return new VendorDraftView(
            Text.Truncate(item.Code, 120),
            item.Title,
            $"{item.Archived}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorDraftView> ToVendorDraftViews(List<VendorDraft> items)
    {
        var views = new List<VendorDraftView>();
        foreach (var item in items)
        {
            views.Add(ToVendorDraftView(item));
        }
        return views;
    }

    public static double SumVendorDraftView(VendorDraftView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedVersion + parsedMaximum;
    }
}
