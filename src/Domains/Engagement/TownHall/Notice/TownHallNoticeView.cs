namespace Acme.Workforce.Engagement.TownHall.Notice;

using System.Globalization;

/// <summary>How screens and exports show a town hall notice.</summary>
public sealed record TownHallNoticeView(string Code, string Name, string Discount, string Notes, string AttemptsText, string Rating, string StatusLabel)
{
    public static TownHallNoticeView ToTownHallNoticeView(TownHallNotice item)
    {
        return new TownHallNoticeView(
            item.Code,
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallNoticeView> ToTownHallNoticeViews(List<TownHallNotice> items)
    {
        var views = new List<TownHallNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallNoticeView(item));
        }
        return views;
    }

    public static double SumTownHallNoticeView(TownHallNoticeView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedAttempts + parsedRating;
    }
}
