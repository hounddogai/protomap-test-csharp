namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using System.Globalization;

/// <summary>How screens and exports show a manual recognition schedule.</summary>
public sealed record ManualRecognitionScheduleView(string Code, string Title, string Location, string Rating, string Approved, string StatusLabel)
{
    public static ManualRecognitionScheduleView ToManualRecognitionScheduleView(ManualRecognitionSchedule item)
    {
        return new ManualRecognitionScheduleView(
            item.Code,
            item.Title,
            item.Location,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualRecognitionScheduleView> ToManualRecognitionScheduleViews(List<ManualRecognitionSchedule> items)
    {
        var views = new List<ManualRecognitionScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToManualRecognitionScheduleView(item));
        }
        return views;
    }

    public static double SumManualRecognitionScheduleView(ManualRecognitionScheduleView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
