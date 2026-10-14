namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending visitor adjustment.</summary>
public sealed record PendingVisitorAdjustmentView(string Code, string TitleText, string RegionText, string Margin, string Description, string VersionText, string CreatedAt, string LeaveDays, string StatusLabel)
{
    public static PendingVisitorAdjustmentView ToPendingVisitorAdjustmentView(PendingVisitorAdjustment item)
    {
        return new PendingVisitorAdjustmentView(
            item.Code,
            item.Title,
            Text.Truncate(item.Region, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingVisitorAdjustmentView> ToPendingVisitorAdjustmentViews(List<PendingVisitorAdjustment> items)
    {
        var views = new List<PendingVisitorAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToPendingVisitorAdjustmentView(item));
        }
        return views;
    }

    public static double SumPendingVisitorAdjustmentView(PendingVisitorAdjustmentView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedVersion + (double)parsedCreatedAt;
    }
}
