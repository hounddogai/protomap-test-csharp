namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using System.Globalization;

/// <summary>How screens and exports show a adaptive training due metric.</summary>
public sealed record AdaptiveTrainingDueMetricView(string Code, string Label, string Sequence, string Checksum, string WeightText, string StatusLabel)
{
    public static AdaptiveTrainingDueMetricView ToAdaptiveTrainingDueMetricView(AdaptiveTrainingDueMetric item)
    {
        return new AdaptiveTrainingDueMetricView(
            item.Code,
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTrainingDueMetricView> ToAdaptiveTrainingDueMetricViews(List<AdaptiveTrainingDueMetric> items)
    {
        var views = new List<AdaptiveTrainingDueMetricView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTrainingDueMetricView(item));
        }
        return views;
    }

    public static double SumAdaptiveTrainingDueMetricView(AdaptiveTrainingDueMetricView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedWeight;
    }
}
