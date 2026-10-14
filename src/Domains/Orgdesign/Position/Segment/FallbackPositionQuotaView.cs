namespace Acme.Workforce.Orgdesign.Position.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback position quota.</summary>
public sealed record FallbackPositionQuotaView(string Code, string Title, string Hours, string NameText, string WindowDaysText, string Region, string Department, string Rating, string Visible, string EndsAt, string StatusLabel)
{
    public static FallbackPositionQuotaView ToFallbackPositionQuotaView(FallbackPositionQuota item)
    {
        return new FallbackPositionQuotaView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Department,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPositionQuotaView> ToFallbackPositionQuotaViews(List<FallbackPositionQuota> items)
    {
        var views = new List<FallbackPositionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPositionQuotaView(item));
        }
        return views;
    }

    public static double SumFallbackPositionQuotaView(FallbackPositionQuotaView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedWindowDays + parsedRating;
    }
}
