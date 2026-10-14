namespace Acme.Workforce.Absence.LeaveRequest.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted leave request adjustment.</summary>
public sealed record WeightedLeaveRequestAdjustmentView(string Code, string LabelText, string TitleText, string Description, string Checksum, string Hours, string Score, string Source, string Phone, string QuantityText, string Retries, string StatusLabel)
{
    public static WeightedLeaveRequestAdjustmentView ToWeightedLeaveRequestAdjustmentView(WeightedLeaveRequestAdjustment item)
    {
        return new WeightedLeaveRequestAdjustmentView(
            item.Code,
            item.Label,
            item.Title,
            Text.Truncate(item.Description, 80),
            Text.Truncate(item.Checksum, 64),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            Text.Truncate(item.Phone, 80),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedLeaveRequestAdjustmentView> ToWeightedLeaveRequestAdjustmentViews(List<WeightedLeaveRequestAdjustment> items)
    {
        var views = new List<WeightedLeaveRequestAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedLeaveRequestAdjustmentView(item));
        }
        return views;
    }

    public static double SumWeightedLeaveRequestAdjustmentView(WeightedLeaveRequestAdjustmentView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedHours + parsedScore + (double)parsedQuantity;
    }
}
