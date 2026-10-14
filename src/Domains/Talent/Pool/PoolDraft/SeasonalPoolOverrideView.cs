namespace Acme.Workforce.Talent.Pool.PoolDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal pool override.</summary>
public sealed record SeasonalPoolOverrideView(string Code, string Title, string Reviewer, string Skill, string Capacity, string ManagerId, string Active, string BadgeText, string Category, string StatusLabel)
{
    public static SeasonalPoolOverrideView ToSeasonalPoolOverrideView(SeasonalPoolOverride item)
    {
        return new SeasonalPoolOverrideView(
            item.Code,
            item.Title,
            item.Reviewer,
            item.Skill,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Active}",
            item.Badge,
            Text.Truncate(item.Category, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPoolOverrideView> ToSeasonalPoolOverrideViews(List<SeasonalPoolOverride> items)
    {
        var views = new List<SeasonalPoolOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPoolOverrideView(item));
        }
        return views;
    }

    public static double SumSeasonalPoolOverrideView(SeasonalPoolOverrideView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
