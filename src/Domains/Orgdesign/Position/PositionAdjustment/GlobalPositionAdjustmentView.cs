namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using System.Globalization;

/// <summary>How screens and exports show a global position adjustment.</summary>
public sealed record GlobalPositionAdjustmentView(string Code, string Name, string Amount, string ArchivedText, string ScoreText, string SegmentText, string Priority, string RatingText, string StatusLabel)
{
    public static GlobalPositionAdjustmentView ToGlobalPositionAdjustmentView(GlobalPositionAdjustment item)
    {
        return new GlobalPositionAdjustmentView(
            item.Code,
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPositionAdjustmentView> ToGlobalPositionAdjustmentViews(List<GlobalPositionAdjustment> items)
    {
        var views = new List<GlobalPositionAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPositionAdjustmentView(item));
        }
        return views;
    }

    public static double SumGlobalPositionAdjustmentView(GlobalPositionAdjustmentView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedAmount + parsedScore + (double)parsedPriority;
    }
}
