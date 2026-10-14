namespace Acme.Workforce.Orgdesign.Span.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict span claim.</summary>
public sealed record StrictSpanClaimView(string Code, string Name, string BatchSize, string Quantity, string Title, string Ratio, string RatingText, string StatusLabel)
{
    public static StrictSpanClaimView ToStrictSpanClaimView(StrictSpanClaim item)
    {
        return new StrictSpanClaimView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictSpanClaimView> ToStrictSpanClaimViews(List<StrictSpanClaim> items)
    {
        var views = new List<StrictSpanClaimView>();
        foreach (var item in items)
        {
            views.Add(ToStrictSpanClaimView(item));
        }
        return views;
    }

    public static double SumStrictSpanClaimView(StrictSpanClaimView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedQuantity + parsedRatio;
    }
}
