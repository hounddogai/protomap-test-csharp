namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan target.</summary>
public sealed record SuccessionPlanTargetView(string Code, string Name, string Approved, string Threshold, string UpdatedAtText, string SalaryBand, string CostCenter, string TeamText, string Limit, string ScoreText, string StatusLabel)
{
    public static SuccessionPlanTargetView ToSuccessionPlanTargetView(SuccessionPlanTarget item)
    {
        return new SuccessionPlanTargetView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.CostCenter,
            item.Team,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanTargetView> ToSuccessionPlanTargetViews(List<SuccessionPlanTarget> items)
    {
        var views = new List<SuccessionPlanTargetView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanTargetView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanTargetView(SuccessionPlanTargetView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedUpdatedAt + (double)parsedLimit;
    }
}
