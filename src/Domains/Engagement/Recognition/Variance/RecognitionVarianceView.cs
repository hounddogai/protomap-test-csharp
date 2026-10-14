namespace Acme.Workforce.Engagement.Recognition.Variance;

using System.Globalization;

/// <summary>How screens and exports show a recognition variance.</summary>
public sealed record RecognitionVarianceView(string CodeText, string Title, string Pinned, string Rate, string Ratio, string StatusLabel)
{
    public static RecognitionVarianceView ToRecognitionVarianceView(RecognitionVariance item)
    {
        return new RecognitionVarianceView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionVarianceView> ToRecognitionVarianceViews(List<RecognitionVariance> items)
    {
        var views = new List<RecognitionVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionVarianceView(item));
        }
        return views;
    }

    public static double SumRecognitionVarianceView(RecognitionVarianceView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRate + parsedRatio;
    }
}
