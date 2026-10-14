namespace Acme.Workforce.Compensation.Adjustment.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global adjustment score.</summary>
public sealed record GlobalAdjustmentScoreView(string CodeText, string Title, string SalaryBand, string SegmentText, string EmployeeId, string RatingText, string Notes, string FloorValue, string WindowDaysText, string CostCenter, string StatusLabel)
{
    public static GlobalAdjustmentScoreView ToGlobalAdjustmentScoreView(GlobalAdjustmentScore item)
    {
        return new GlobalAdjustmentScoreView(
            Text.Truncate(item.Code, 80),
            item.Title,
            Text.Truncate(item.SalaryBand, 120),
            item.Segment,
            item.EmployeeId,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAdjustmentScoreView> ToGlobalAdjustmentScoreViews(List<GlobalAdjustmentScore> items)
    {
        var views = new List<GlobalAdjustmentScoreView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAdjustmentScoreView(item));
        }
        return views;
    }

    public static double SumGlobalAdjustmentScoreView(GlobalAdjustmentScoreView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return parsedRating + parsedFloorValue + (double)parsedWindowDays;
    }
}
