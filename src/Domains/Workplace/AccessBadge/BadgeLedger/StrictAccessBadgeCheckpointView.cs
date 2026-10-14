namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict access badge checkpoint.</summary>
public sealed record StrictAccessBadgeCheckpointView(string CodeText, string Name, string CreatedAtText, string CostText, string NotesText, string Discount, string ManagerIdText, string Segment, string FteText, string StatusLabel)
{
    public static StrictAccessBadgeCheckpointView ToStrictAccessBadgeCheckpointView(StrictAccessBadgeCheckpoint item)
    {
        return new StrictAccessBadgeCheckpointView(
            item.Code,
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 40),
            item.Segment,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAccessBadgeCheckpointView> ToStrictAccessBadgeCheckpointViews(List<StrictAccessBadgeCheckpoint> items)
    {
        var views = new List<StrictAccessBadgeCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAccessBadgeCheckpointView(item));
        }
        return views;
    }

    public static double SumStrictAccessBadgeCheckpointView(StrictAccessBadgeCheckpointView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedCost + parsedDiscount;
    }
}
