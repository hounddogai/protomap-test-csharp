namespace Acme.Workforce.Engagement.Recognition.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback recognition digest.</summary>
public sealed record FallbackRecognitionDigestView(string Code, string Label, string Ceiling, string CreatedAt, string Quantity, string Rating, string StatusLabel)
{
    public static FallbackRecognitionDigestView ToFallbackRecognitionDigestView(FallbackRecognitionDigest item)
    {
        return new FallbackRecognitionDigestView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackRecognitionDigestView> ToFallbackRecognitionDigestViews(List<FallbackRecognitionDigest> items)
    {
        var views = new List<FallbackRecognitionDigestView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackRecognitionDigestView(item));
        }
        return views;
    }

    public static double SumFallbackRecognitionDigestView(FallbackRecognitionDigestView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedCreatedAt + (double)parsedQuantity;
    }
}
