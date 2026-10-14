namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due rollup.</summary>
public sealed record TrainingDueRollupView(string Code, string Label, string CostText, string JobCode, string OwnerId, string Margin, string BadgeText, string StatusLabel)
{
    public static TrainingDueRollupView ToTrainingDueRollupView(TrainingDueRollup item)
    {
        return new TrainingDueRollupView(
            item.Code,
            item.Label,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            Text.Truncate(item.OwnerId, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueRollupView> ToTrainingDueRollupViews(List<TrainingDueRollup> items)
    {
        var views = new List<TrainingDueRollupView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueRollupView(item));
        }
        return views;
    }

    public static double SumTrainingDueRollupView(TrainingDueRollupView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedCost + parsedMargin;
    }
}
