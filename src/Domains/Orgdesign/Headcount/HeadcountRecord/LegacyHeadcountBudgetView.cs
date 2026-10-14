namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy headcount budget.</summary>
public sealed record LegacyHeadcountBudgetView(string Code, string Title, string DurationMinutesText, string ScoreText, string Rating, string Phone, string ActiveText, string NameText, string StatusLabel)
{
    public static LegacyHeadcountBudgetView ToLegacyHeadcountBudgetView(LegacyHeadcountBudget item)
    {
        return new LegacyHeadcountBudgetView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 80),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Active}",
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyHeadcountBudgetView> ToLegacyHeadcountBudgetViews(List<LegacyHeadcountBudget> items)
    {
        var views = new List<LegacyHeadcountBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyHeadcountBudgetView(item));
        }
        return views;
    }

    public static double SumLegacyHeadcountBudgetView(LegacyHeadcountBudgetView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedScore + parsedRating;
    }
}
