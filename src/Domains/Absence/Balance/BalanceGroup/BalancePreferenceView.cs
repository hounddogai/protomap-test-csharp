namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using System.Globalization;

/// <summary>How screens and exports show a balance preference.</summary>
public sealed record BalancePreferenceView(string Code, string TitleText, string Pinned, string Rating, string FloorValue, string HoursText, string Margin, string Currency, string Unit, string Score, string UpdatedAtText, string StatusLabel)
{
    public static BalancePreferenceView ToBalancePreferenceView(BalancePreference item)
    {
        return new BalancePreferenceView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Unit,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalancePreferenceView> ToBalancePreferenceViews(List<BalancePreference> items)
    {
        var views = new List<BalancePreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToBalancePreferenceView(item));
        }
        return views;
    }

    public static double SumBalancePreferenceView(BalancePreferenceView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedRating + parsedFloorValue + parsedHours;
    }
}
