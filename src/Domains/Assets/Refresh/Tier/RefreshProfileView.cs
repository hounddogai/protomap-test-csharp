namespace Acme.Workforce.Assets.Refresh.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh profile.</summary>
public sealed record RefreshProfileView(string CodeText, string TitleText, string DurationMinutes, string Grade, string Rating, string StatusLabel)
{
    public static RefreshProfileView ToRefreshProfileView(RefreshProfile item)
    {
        return new RefreshProfileView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 80),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshProfileView> ToRefreshProfileViews(List<RefreshProfile> items)
    {
        var views = new List<RefreshProfileView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshProfileView(item));
        }
        return views;
    }

    public static double SumRefreshProfileView(RefreshProfileView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRating;
    }
}
