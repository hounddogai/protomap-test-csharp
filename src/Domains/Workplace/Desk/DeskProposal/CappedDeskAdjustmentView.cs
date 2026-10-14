namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped desk adjustment.</summary>
public sealed record CappedDeskAdjustmentView(string CodeText, string LabelText, string ThresholdText, string DurationMinutesText, string Description, string Ratio, string Badge, string Budget, string SalaryBandText, string Attempts, string Category, string StatusLabel)
{
    public static CappedDeskAdjustmentView ToCappedDeskAdjustmentView(CappedDeskAdjustment item)
    {
        return new CappedDeskAdjustmentView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedDeskAdjustmentView> ToCappedDeskAdjustmentViews(List<CappedDeskAdjustment> items)
    {
        var views = new List<CappedDeskAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToCappedDeskAdjustmentView(item));
        }
        return views;
    }

    public static double SumCappedDeskAdjustmentView(CappedDeskAdjustmentView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedDurationMinutes + parsedRatio;
    }
}
