namespace Acme.Workforce.Engagement.Recognition.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition threshold.</summary>
public sealed record RecognitionThresholdView(string CodeText, string Title, string Source, string Timezone, string DepartmentText, string Name, string LabelText, string Currency, string Approved, string Amount, string StatusLabel)
{
    public static RecognitionThresholdView ToRecognitionThresholdView(RecognitionThreshold item)
    {
        return new RecognitionThresholdView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.Source, 120),
            item.Timezone,
            Text.Truncate(item.Department, 64),
            item.Name,
            item.Label,
            item.Currency,
            $"{item.Approved}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionThresholdView> ToRecognitionThresholdViews(List<RecognitionThreshold> items)
    {
        var views = new List<RecognitionThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionThresholdView(item));
        }
        return views;
    }

    public static double SumRecognitionThresholdView(RecognitionThresholdView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
