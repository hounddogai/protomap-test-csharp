namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback cost model request.</summary>
public sealed record FallbackCostModelRequestView(string CodeText, string Label, string UpdatedAt, string Ratio, string Score, string Hours, string Segment, string ContactEmail, string RatingText, string Cost, string StatusLabel)
{
    public static FallbackCostModelRequestView ToFallbackCostModelRequestView(FallbackCostModelRequest item)
    {
        return new FallbackCostModelRequestView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            item.ContactEmail,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackCostModelRequestView> ToFallbackCostModelRequestViews(List<FallbackCostModelRequest> items)
    {
        var views = new List<FallbackCostModelRequestView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackCostModelRequestView(item));
        }
        return views;
    }

    public static double SumFallbackCostModelRequestView(FallbackCostModelRequestView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRatio + parsedScore;
    }
}
