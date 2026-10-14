namespace Acme.Workforce.Orgdesign.Span.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback span score.</summary>
public sealed record FallbackSpanScoreView(string Code, string Name, string Region, string GraceDays, string Reviewer, string StatusLabel)
{
    public static FallbackSpanScoreView ToFallbackSpanScoreView(FallbackSpanScore item)
    {
        return new FallbackSpanScoreView(
            Text.Truncate(item.Code, 80),
            item.Name,
            Text.Truncate(item.Region, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackSpanScoreView> ToFallbackSpanScoreViews(List<FallbackSpanScore> items)
    {
        var views = new List<FallbackSpanScoreView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackSpanScoreView(item));
        }
        return views;
    }

    public static double SumFallbackSpanScoreView(FallbackSpanScoreView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays;
    }
}
