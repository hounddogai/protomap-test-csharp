namespace Acme.Workforce.Orgdesign.Position.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position budget.</summary>
public sealed record PositionBudgetView(string Code, string NameText, string Timezone, string Limit, string ReviewerText, string Rating, string VersionText, string Approved, string Ratio, string Margin, string StatusLabel)
{
    public static PositionBudgetView ToPositionBudgetView(PositionBudget item)
    {
        return new PositionBudgetView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 120),
            item.Timezone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionBudgetView> ToPositionBudgetViews(List<PositionBudget> items)
    {
        var views = new List<PositionBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToPositionBudgetView(item));
        }
        return views;
    }

    public static double SumPositionBudgetView(PositionBudgetView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedRating + (double)parsedVersion;
    }
}
