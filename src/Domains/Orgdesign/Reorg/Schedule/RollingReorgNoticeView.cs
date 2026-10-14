namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling reorg notice.</summary>
public sealed record RollingReorgNoticeView(string Code, string Label, string Phone, string Name, string PinnedText, string RatingText, string Margin, string RegionText, string StatusLabel)
{
    public static RollingReorgNoticeView ToRollingReorgNoticeView(RollingReorgNotice item)
    {
        return new RollingReorgNoticeView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Phone,
            item.Name,
            $"{item.Pinned}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingReorgNoticeView> ToRollingReorgNoticeViews(List<RollingReorgNotice> items)
    {
        var views = new List<RollingReorgNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToRollingReorgNoticeView(item));
        }
        return views;
    }

    public static double SumRollingReorgNoticeView(RollingReorgNoticeView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedRating + parsedMargin;
    }
}
