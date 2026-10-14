namespace Acme.Workforce.Assets.Assignment.Batch;

using System.Globalization;

/// <summary>How screens and exports show a default assignment batch.</summary>
public sealed record DefaultAssignmentBatchView(string Code, string Name, string WindowDaysText, string Quantity, string ThresholdText, string Ratio, string ScoreText, string EmployeeId, string Skill, string CostText, string StatusLabel)
{
    public static DefaultAssignmentBatchView ToDefaultAssignmentBatchView(DefaultAssignmentBatch item)
    {
        return new DefaultAssignmentBatchView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Skill,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAssignmentBatchView> ToDefaultAssignmentBatchViews(List<DefaultAssignmentBatch> items)
    {
        var views = new List<DefaultAssignmentBatchView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAssignmentBatchView(item));
        }
        return views;
    }

    public static double SumDefaultAssignmentBatchView(DefaultAssignmentBatchView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedQuantity + parsedThreshold;
    }
}
