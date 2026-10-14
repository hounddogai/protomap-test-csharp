namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted access badge batch.</summary>
public sealed record WeightedAccessBadgeBatchView(string Code, string LabelText, string Archived, string GraceDays, string Checksum, string EndsAt, string TitleText, string Weight, string Capacity, string StatusLabel)
{
    public static WeightedAccessBadgeBatchView ToWeightedAccessBadgeBatchView(WeightedAccessBadgeBatch item)
    {
        return new WeightedAccessBadgeBatchView(
            item.Code,
            Text.Truncate(item.Label, 120),
            $"{item.Archived}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAccessBadgeBatchView> ToWeightedAccessBadgeBatchViews(List<WeightedAccessBadgeBatch> items)
    {
        var views = new List<WeightedAccessBadgeBatchView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAccessBadgeBatchView(item));
        }
        return views;
    }

    public static double SumWeightedAccessBadgeBatchView(WeightedAccessBadgeBatchView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedEndsAt + parsedWeight;
    }
}
