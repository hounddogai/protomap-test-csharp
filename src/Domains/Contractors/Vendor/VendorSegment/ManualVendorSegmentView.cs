namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual vendor segment.</summary>
public sealed record ManualVendorSegmentView(string Code, string Title, string Locale, string Channel, string Priority, string EmployeeIdText, string CostCenter, string Maximum, string Notes, string StatusLabel)
{
    public static ManualVendorSegmentView ToManualVendorSegmentView(ManualVendorSegment item)
    {
        return new ManualVendorSegmentView(
            item.Code,
            item.Title,
            item.Locale,
            item.Channel,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            item.CostCenter,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualVendorSegmentView> ToManualVendorSegmentViews(List<ManualVendorSegment> items)
    {
        var views = new List<ManualVendorSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToManualVendorSegmentView(item));
        }
        return views;
    }

    public static double SumManualVendorSegmentView(ManualVendorSegmentView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedMaximum;
    }
}
