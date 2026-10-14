namespace Acme.Workforce.Engagement.Recognition.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition adjustment.</summary>
public sealed record RecognitionAdjustmentView(string Code, string Name, string Attempts, string Reviewer, string Ceiling, string Cost, string MaximumText, string Description, string Timezone, string SequenceText, string ManagerId, string StatusLabel)
{
    public static RecognitionAdjustmentView ToRecognitionAdjustmentView(RecognitionAdjustment item)
    {
        return new RecognitionAdjustmentView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 80),
            item.Timezone,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionAdjustmentView> ToRecognitionAdjustmentViews(List<RecognitionAdjustment> items)
    {
        var views = new List<RecognitionAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionAdjustmentView(item));
        }
        return views;
    }

    public static double SumRecognitionAdjustmentView(RecognitionAdjustmentView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedCeiling + parsedCost;
    }
}
