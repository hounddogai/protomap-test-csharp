namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due threshold.</summary>
public sealed record TrainingDueThresholdView(string CodeText, string Label, string AttemptsText, string Cost, string Country, string Ceiling, string RetriesText, string Fte, string StatusLabel)
{
    public static TrainingDueThresholdView ToTrainingDueThresholdView(TrainingDueThreshold item)
    {
        return new TrainingDueThresholdView(
            item.Code,
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueThresholdView> ToTrainingDueThresholdViews(List<TrainingDueThreshold> items)
    {
        var views = new List<TrainingDueThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueThresholdView(item));
        }
        return views;
    }

    public static double SumTrainingDueThresholdView(TrainingDueThresholdView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedCost + parsedCeiling;
    }
}
