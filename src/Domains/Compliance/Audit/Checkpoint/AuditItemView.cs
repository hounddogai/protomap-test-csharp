namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit item.</summary>
public sealed record AuditItemView(string Code, string Label, string Pinned, string Title, string VisibleText, string MinimumText, string Threshold, string CapacityText, string ReviewerText, string Notes, string MarginText, string StatusLabel)
{
    public static AuditItemView ToAuditItemView(AuditItem item)
    {
        return new AuditItemView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 64),
            $"{item.Pinned}",
            item.Title,
            $"{item.Visible}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Notes,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditItemView> ToAuditItemViews(List<AuditItem> items)
    {
        var views = new List<AuditItemView>();
        foreach (var item in items)
        {
            views.Add(ToAuditItemView(item));
        }
        return views;
    }

    public static double SumAuditItemView(AuditItemView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedThreshold + (double)parsedCapacity;
    }
}
