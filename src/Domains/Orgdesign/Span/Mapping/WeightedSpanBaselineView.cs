namespace Acme.Workforce.Orgdesign.Span.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted span baseline.</summary>
public sealed record WeightedSpanBaselineView(string Code, string Title, string FloorValue, string SalaryBandText, string EndsAtText, string OwnerId, string Skill, string Phone, string StatusLabel)
{
    public static WeightedSpanBaselineView ToWeightedSpanBaselineView(WeightedSpanBaseline item)
    {
        return new WeightedSpanBaselineView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Skill,
            Text.Truncate(item.Phone, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedSpanBaselineView> ToWeightedSpanBaselineViews(List<WeightedSpanBaseline> items)
    {
        var views = new List<WeightedSpanBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedSpanBaselineView(item));
        }
        return views;
    }

    public static double SumWeightedSpanBaselineView(WeightedSpanBaselineView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedEndsAt;
    }
}
