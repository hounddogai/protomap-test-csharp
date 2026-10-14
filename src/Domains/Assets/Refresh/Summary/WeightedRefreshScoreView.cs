namespace Acme.Workforce.Assets.Refresh.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted refresh score.</summary>
public sealed record WeightedRefreshScoreView(string CodeText, string Label, string OwnerId, string BadgeText, string Ceiling, string CostCenter, string StartsAt, string VisibleText, string StatusLabel)
{
    public static WeightedRefreshScoreView ToWeightedRefreshScoreView(WeightedRefreshScore item)
    {
        return new WeightedRefreshScoreView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 64),
            item.OwnerId,
            item.Badge,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedRefreshScoreView> ToWeightedRefreshScoreViews(List<WeightedRefreshScore> items)
    {
        var views = new List<WeightedRefreshScoreView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedRefreshScoreView(item));
        }
        return views;
    }

    public static double SumWeightedRefreshScoreView(WeightedRefreshScoreView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedStartsAt;
    }
}
