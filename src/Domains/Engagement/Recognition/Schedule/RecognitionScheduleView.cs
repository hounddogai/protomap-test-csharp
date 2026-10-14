namespace Acme.Workforce.Engagement.Recognition.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition schedule.</summary>
public sealed record RecognitionScheduleView(string Code, string TitleText, string DescriptionText, string Timezone, string GraceDaysText, string StatusLabel)
{
    public static RecognitionScheduleView ToRecognitionScheduleView(RecognitionSchedule item)
    {
        return new RecognitionScheduleView(
            item.Code,
            item.Title,
            Text.Truncate(item.Description, 120),
            item.Timezone,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionScheduleView> ToRecognitionScheduleViews(List<RecognitionSchedule> items)
    {
        var views = new List<RecognitionScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionScheduleView(item));
        }
        return views;
    }

    public static double SumRecognitionScheduleView(RecognitionScheduleView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays;
    }
}
