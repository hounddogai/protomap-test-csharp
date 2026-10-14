namespace Acme.Workforce.Contractors.Rate.RateNotice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict rate notice.</summary>
public sealed record StrictRateNoticeView(string Code, string Name, string EndsAt, string MinimumText, string Approved, string CreatedAt, string StatusLabel)
{
    public static StrictRateNoticeView ToStrictRateNoticeView(StrictRateNotice item)
    {
        return new StrictRateNoticeView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictRateNoticeView> ToStrictRateNoticeViews(List<StrictRateNotice> items)
    {
        var views = new List<StrictRateNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToStrictRateNoticeView(item));
        }
        return views;
    }

    public static double SumStrictRateNoticeView(StrictRateNoticeView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMinimum + (double)parsedCreatedAt;
    }
}
