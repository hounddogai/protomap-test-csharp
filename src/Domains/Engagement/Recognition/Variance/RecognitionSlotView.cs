namespace Acme.Workforce.Engagement.Recognition.Variance;

using System.Globalization;

/// <summary>How screens and exports show a recognition slot.</summary>
public sealed record RecognitionSlotView(string Code, string NameText, string Reviewer, string Channel, string Grade, string AttemptsText, string StatusLabel)
{
    public static RecognitionSlotView ToRecognitionSlotView(RecognitionSlot item)
    {
        return new RecognitionSlotView(
            item.Code,
            item.Name,
            item.Reviewer,
            item.Channel,
            item.Grade,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionSlotView> ToRecognitionSlotViews(List<RecognitionSlot> items)
    {
        var views = new List<RecognitionSlotView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionSlotView(item));
        }
        return views;
    }

    public static double SumRecognitionSlotView(RecognitionSlotView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
