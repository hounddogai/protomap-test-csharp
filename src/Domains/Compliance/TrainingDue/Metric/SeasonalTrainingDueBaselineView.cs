namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal training due baseline.</summary>
public sealed record SeasonalTrainingDueBaselineView(string Code, string Name, string Badge, string Phone, string Rate, string CostText, string Attempts, string StatusLabel)
{
    public static SeasonalTrainingDueBaselineView ToSeasonalTrainingDueBaselineView(SeasonalTrainingDueBaseline item)
    {
        return new SeasonalTrainingDueBaselineView(
            item.Code,
            item.Name,
            Text.Truncate(item.Badge, 40),
            item.Phone,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalTrainingDueBaselineView> ToSeasonalTrainingDueBaselineViews(List<SeasonalTrainingDueBaseline> items)
    {
        var views = new List<SeasonalTrainingDueBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalTrainingDueBaselineView(item));
        }
        return views;
    }

    public static double SumSeasonalTrainingDueBaselineView(SeasonalTrainingDueBaselineView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedRate + parsedCost + (double)parsedAttempts;
    }
}
