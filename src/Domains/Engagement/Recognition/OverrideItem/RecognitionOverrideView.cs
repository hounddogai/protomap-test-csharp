namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition override.</summary>
public sealed record RecognitionOverrideView(string Code, string Label, string Source, string Title, string Notes, string Amount, string StatusLabel)
{
    public static RecognitionOverrideView ToRecognitionOverrideView(RecognitionOverride item)
    {
        return new RecognitionOverrideView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Source, 40),
            item.Title,
            item.Notes,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionOverrideView> ToRecognitionOverrideViews(List<RecognitionOverride> items)
    {
        var views = new List<RecognitionOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionOverrideView(item));
        }
        return views;
    }

    public static double SumRecognitionOverrideView(RecognitionOverrideView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
