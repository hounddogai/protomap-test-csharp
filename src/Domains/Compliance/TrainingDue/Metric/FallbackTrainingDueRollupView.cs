namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback training due rollup.</summary>
public sealed record FallbackTrainingDueRollupView(string CodeText, string TitleText, string GraceDays, string Source, string RegionText, string StatusLabel)
{
    public static FallbackTrainingDueRollupView ToFallbackTrainingDueRollupView(FallbackTrainingDueRollup item)
    {
        return new FallbackTrainingDueRollupView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackTrainingDueRollupView> ToFallbackTrainingDueRollupViews(List<FallbackTrainingDueRollup> items)
    {
        var views = new List<FallbackTrainingDueRollupView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackTrainingDueRollupView(item));
        }
        return views;
    }

    public static double SumFallbackTrainingDueRollupView(FallbackTrainingDueRollupView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays;
    }
}
