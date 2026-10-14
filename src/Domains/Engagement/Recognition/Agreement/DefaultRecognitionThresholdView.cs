namespace Acme.Workforce.Engagement.Recognition.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default recognition threshold.</summary>
public sealed record DefaultRecognitionThresholdView(string CodeText, string Title, string Channel, string StartsAt, string SalaryBand, string FloorValueText, string Rating, string StatusLabel)
{
    public static DefaultRecognitionThresholdView ToDefaultRecognitionThresholdView(DefaultRecognitionThreshold item)
    {
        return new DefaultRecognitionThresholdView(
            item.Code,
            item.Title,
            Text.Truncate(item.Channel, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultRecognitionThresholdView> ToDefaultRecognitionThresholdViews(List<DefaultRecognitionThreshold> items)
    {
        var views = new List<DefaultRecognitionThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultRecognitionThresholdView(item));
        }
        return views;
    }

    public static double SumDefaultRecognitionThresholdView(DefaultRecognitionThresholdView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedFloorValue + parsedRating;
    }
}
