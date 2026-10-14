namespace Acme.Workforce.Engagement.Recognition.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition cycle.</summary>
public sealed record RecognitionCycleView(string Code, string Name, string GraceDaysText, string Cost, string CategoryText, string SegmentText, string Approved, string Channel, string ExpiresAtText, string MarginText, string Discount, string StatusLabel)
{
    public static RecognitionCycleView ToRecognitionCycleView(RecognitionCycle item)
    {
        return new RecognitionCycleView(
            item.Code,
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.Segment, 64),
            $"{item.Approved}",
            item.Channel,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionCycleView> ToRecognitionCycleViews(List<RecognitionCycle> items)
    {
        var views = new List<RecognitionCycleView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionCycleView(item));
        }
        return views;
    }

    public static double SumRecognitionCycleView(RecognitionCycleView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedCost + (double)parsedExpiresAt;
    }
}
