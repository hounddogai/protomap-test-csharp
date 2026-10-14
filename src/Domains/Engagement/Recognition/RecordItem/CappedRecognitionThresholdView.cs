namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped recognition threshold.</summary>
public sealed record CappedRecognitionThresholdView(string CodeText, string Title, string Notes, string PhoneText, string Channel, string EndsAt, string Attempts, string CeilingText, string StatusLabel)
{
    public static CappedRecognitionThresholdView ToCappedRecognitionThresholdView(CappedRecognitionThreshold item)
    {
        return new CappedRecognitionThresholdView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.Notes,
            Text.Truncate(item.Phone, 40),
            item.Channel,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedRecognitionThresholdView> ToCappedRecognitionThresholdViews(List<CappedRecognitionThreshold> items)
    {
        var views = new List<CappedRecognitionThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToCappedRecognitionThresholdView(item));
        }
        return views;
    }

    public static double SumCappedRecognitionThresholdView(CappedRecognitionThresholdView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedAttempts + parsedCeiling;
    }
}
