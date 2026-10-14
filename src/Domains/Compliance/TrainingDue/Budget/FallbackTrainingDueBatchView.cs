namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback training due batch.</summary>
public sealed record FallbackTrainingDueBatchView(string CodeText, string TitleText, string CostCenter, string TimezoneText, string ExternalRefText, string Threshold, string Region, string StatusLabel)
{
    public static FallbackTrainingDueBatchView ToFallbackTrainingDueBatchView(FallbackTrainingDueBatch item)
    {
        return new FallbackTrainingDueBatchView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.CostCenter, 40),
            Text.Truncate(item.Timezone, 80),
            Text.Truncate(item.ExternalRef, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackTrainingDueBatchView> ToFallbackTrainingDueBatchViews(List<FallbackTrainingDueBatch> items)
    {
        var views = new List<FallbackTrainingDueBatchView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackTrainingDueBatchView(item));
        }
        return views;
    }

    public static double SumFallbackTrainingDueBatchView(FallbackTrainingDueBatchView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
