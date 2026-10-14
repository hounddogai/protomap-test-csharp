namespace Acme.Workforce.Engagement.Recognition.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition batch.</summary>
public sealed record RecognitionBatchView(string CodeText, string Label, string Threshold, string PinnedText, string Hours, string Source, string Timezone, string Reviewer, string Maximum, string StatusLabel)
{
    public static RecognitionBatchView ToRecognitionBatchView(RecognitionBatch item)
    {
        return new RecognitionBatchView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            item.Timezone,
            item.Reviewer,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionBatchView> ToRecognitionBatchViews(List<RecognitionBatch> items)
    {
        var views = new List<RecognitionBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionBatchView(item));
        }
        return views;
    }

    public static double SumRecognitionBatchView(RecognitionBatchView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedHours + parsedMaximum;
    }
}
