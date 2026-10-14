namespace Acme.Workforce.Engagement.Recognition.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional recognition threshold.</summary>
public sealed record RegionalRecognitionThresholdView(string CodeText, string LabelText, string Description, string Phone, string Country, string ScoreText, string StatusLabel)
{
    public static RegionalRecognitionThresholdView ToRegionalRecognitionThresholdView(RegionalRecognitionThreshold item)
    {
        return new RegionalRecognitionThresholdView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Description,
            item.Phone,
            Text.Truncate(item.Country, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalRecognitionThresholdView> ToRegionalRecognitionThresholdViews(List<RegionalRecognitionThreshold> items)
    {
        var views = new List<RegionalRecognitionThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalRecognitionThresholdView(item));
        }
        return views;
    }

    public static double SumRegionalRecognitionThresholdView(RegionalRecognitionThresholdView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
