namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual vendor milestone.</summary>
public sealed record ManualVendorMilestoneView(string Code, string NameText, string Description, string SequenceText, string VersionText, string Margin, string CreatedAt, string ApprovedText, string ChecksumText, string Minimum, string StatusLabel)
{
    public static ManualVendorMilestoneView ToManualVendorMilestoneView(ManualVendorMilestone item)
    {
        return new ManualVendorMilestoneView(
            item.Code,
            item.Name,
            Text.Truncate(item.Description, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Checksum,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualVendorMilestoneView> ToManualVendorMilestoneViews(List<ManualVendorMilestone> items)
    {
        var views = new List<ManualVendorMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToManualVendorMilestoneView(item));
        }
        return views;
    }

    public static double SumManualVendorMilestoneView(ManualVendorMilestoneView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedVersion + parsedMargin;
    }
}
