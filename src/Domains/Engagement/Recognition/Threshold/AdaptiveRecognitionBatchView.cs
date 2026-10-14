namespace Acme.Workforce.Engagement.Recognition.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive recognition batch.</summary>
public sealed record AdaptiveRecognitionBatchView(string CodeText, string Label, string Margin, string MaximumText, string Location, string NotesText, string DescriptionText, string Team, string Budget, string StatusLabel)
{
    public static AdaptiveRecognitionBatchView ToAdaptiveRecognitionBatchView(AdaptiveRecognitionBatch item)
    {
        return new AdaptiveRecognitionBatchView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Notes,
            item.Description,
            item.Team,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveRecognitionBatchView> ToAdaptiveRecognitionBatchViews(List<AdaptiveRecognitionBatch> items)
    {
        var views = new List<AdaptiveRecognitionBatchView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveRecognitionBatchView(item));
        }
        return views;
    }

    public static double SumAdaptiveRecognitionBatchView(AdaptiveRecognitionBatchView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMargin + parsedMaximum + parsedBudget;
    }
}
