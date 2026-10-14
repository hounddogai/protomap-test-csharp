namespace Acme.Workforce.Assets.Warranty.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty baseline.</summary>
public sealed record WarrantyBaselineView(string Code, string Label, string SegmentText, string Phone, string ScoreText, string StatusLabel)
{
    public static WarrantyBaselineView ToWarrantyBaselineView(WarrantyBaseline item)
    {
        return new WarrantyBaselineView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Segment,
            Text.Truncate(item.Phone, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyBaselineView> ToWarrantyBaselineViews(List<WarrantyBaseline> items)
    {
        var views = new List<WarrantyBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyBaselineView(item));
        }
        return views;
    }

    public static double SumWarrantyBaselineView(WarrantyBaselineView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
