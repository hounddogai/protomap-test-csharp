namespace Acme.Workforce.Engagement.Recognition.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition entry.</summary>
public sealed record RecognitionEntryView(string Code, string LabelText, string Minimum, string FloorValue, string VisibleText, string CostCenter, string Reviewer, string Category, string AmountText, string ThresholdText, string MarginText, string StatusLabel)
{
    public static RecognitionEntryView ToRecognitionEntryView(RecognitionEntry item)
    {
        return new RecognitionEntryView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.CostCenter,
            item.Reviewer,
            item.Category,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionEntryView> ToRecognitionEntryViews(List<RecognitionEntry> items)
    {
        var views = new List<RecognitionEntryView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionEntryView(item));
        }
        return views;
    }

    public static double SumRecognitionEntryView(RecognitionEntryView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedFloorValue + parsedAmount;
    }
}
