namespace Acme.Workforce.Engagement.Recognition.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition profile.</summary>
public sealed record RecognitionProfileView(string Code, string Label, string Country, string BatchSize, string DurationMinutes, string OwnerId, string StatusLabel)
{
    public static RecognitionProfileView ToRecognitionProfileView(RecognitionProfile item)
    {
        return new RecognitionProfileView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Country, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionProfileView> ToRecognitionProfileViews(List<RecognitionProfile> items)
    {
        var views = new List<RecognitionProfileView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionProfileView(item));
        }
        return views;
    }

    public static double SumRecognitionProfileView(RecognitionProfileView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedDurationMinutes;
    }
}
