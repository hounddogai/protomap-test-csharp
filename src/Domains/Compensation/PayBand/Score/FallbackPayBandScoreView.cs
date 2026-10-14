namespace Acme.Workforce.Compensation.PayBand.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback pay band score.</summary>
public sealed record FallbackPayBandScoreView(string Code, string Name, string NotesText, string Limit, string LabelText, string Rate, string EmployeeIdText, string Segment, string StatusLabel)
{
    public static FallbackPayBandScoreView ToFallbackPayBandScoreView(FallbackPayBandScore item)
    {
        return new FallbackPayBandScoreView(
            item.Code,
            item.Name,
            item.Notes,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Segment, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayBandScoreView> ToFallbackPayBandScoreViews(List<FallbackPayBandScore> items)
    {
        var views = new List<FallbackPayBandScoreView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayBandScoreView(item));
        }
        return views;
    }

    public static double SumFallbackPayBandScoreView(FallbackPayBandScoreView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedRate;
    }
}
