namespace Acme.Workforce.Engagement.Recognition.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition preference.</summary>
public sealed record RecognitionPreferenceView(string Code, string Label, string ExpiresAt, string EndsAt, string Unit, string StatusLabel)
{
    public static RecognitionPreferenceView ToRecognitionPreferenceView(RecognitionPreference item)
    {
        return new RecognitionPreferenceView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionPreferenceView> ToRecognitionPreferenceViews(List<RecognitionPreference> items)
    {
        var views = new List<RecognitionPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionPreferenceView(item));
        }
        return views;
    }

    public static double SumRecognitionPreferenceView(RecognitionPreferenceView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedEndsAt;
    }
}
