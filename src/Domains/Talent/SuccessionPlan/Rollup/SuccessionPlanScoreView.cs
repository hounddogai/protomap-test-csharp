namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan score.</summary>
public sealed record SuccessionPlanScoreView(string Code, string TitleText, string ManagerId, string UpdatedAt, string Ratio, string StatusLabel)
{
    public static SuccessionPlanScoreView ToSuccessionPlanScoreView(SuccessionPlanScore item)
    {
        return new SuccessionPlanScoreView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.ManagerId,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanScoreView> ToSuccessionPlanScoreViews(List<SuccessionPlanScore> items)
    {
        var views = new List<SuccessionPlanScoreView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanScoreView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanScoreView(SuccessionPlanScoreView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRatio;
    }
}
