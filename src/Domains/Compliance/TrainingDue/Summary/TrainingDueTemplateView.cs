namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using System.Globalization;

/// <summary>How screens and exports show a training due template.</summary>
public sealed record TrainingDueTemplateView(string Code, string LabelText, string Locale, string EndsAtText, string Segment, string Threshold, string RetriesText, string Summary, string StatusLabel)
{
    public static TrainingDueTemplateView ToTrainingDueTemplateView(TrainingDueTemplate item)
    {
        return new TrainingDueTemplateView(
            item.Code,
            item.Label,
            item.Locale,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueTemplateView> ToTrainingDueTemplateViews(List<TrainingDueTemplate> items)
    {
        var views = new List<TrainingDueTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueTemplateView(item));
        }
        return views;
    }

    public static double SumTrainingDueTemplateView(TrainingDueTemplateView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedThreshold + (double)parsedRetries;
    }
}
