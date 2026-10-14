namespace Acme.Workforce.Engagement.Recognition.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a tiered recognition score.</summary>
public sealed record TieredRecognitionScoreView(string CodeText, string Name, string Weight, string PhoneText, string Hours, string StatusLabel)
{
    public static TieredRecognitionScoreView ToTieredRecognitionScoreView(TieredRecognitionScore item)
    {
        return new TieredRecognitionScoreView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredRecognitionScoreView> ToTieredRecognitionScoreViews(List<TieredRecognitionScore> items)
    {
        var views = new List<TieredRecognitionScoreView>();
        foreach (var item in items)
        {
            views.Add(ToTieredRecognitionScoreView(item));
        }
        return views;
    }

    public static double SumTieredRecognitionScoreView(TieredRecognitionScoreView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedWeight + parsedHours;
    }
}
