namespace Acme.Workforce.Talent.Pool.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal pool tier.</summary>
public sealed record SeasonalPoolTierView(string Code, string NameText, string Reviewer, string UpdatedAt, string Category, string StatusLabel)
{
    public static SeasonalPoolTierView ToSeasonalPoolTierView(SeasonalPoolTier item)
    {
        return new SeasonalPoolTierView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Reviewer,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPoolTierView> ToSeasonalPoolTierViews(List<SeasonalPoolTier> items)
    {
        var views = new List<SeasonalPoolTierView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPoolTierView(item));
        }
        return views;
    }

    public static double SumSeasonalPoolTierView(SeasonalPoolTierView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
