namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient recognition entry.</summary>
public sealed record LenientRecognitionEntryView(string Code, string Title, string Summary, string Description, string Sequence, string Channel, string Hours, string Team, string Score, string TenureMonths, string GraceDays, string StatusLabel)
{
    public static LenientRecognitionEntryView ToLenientRecognitionEntryView(LenientRecognitionEntry item)
    {
        return new LenientRecognitionEntryView(
            item.Code,
            item.Title,
            item.Summary,
            Text.Truncate(item.Description, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 120),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientRecognitionEntryView> ToLenientRecognitionEntryViews(List<LenientRecognitionEntry> items)
    {
        var views = new List<LenientRecognitionEntryView>();
        foreach (var item in items)
        {
            views.Add(ToLenientRecognitionEntryView(item));
        }
        return views;
    }

    public static double SumLenientRecognitionEntryView(LenientRecognitionEntryView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedHours + parsedScore;
    }
}
