namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary adjustment assignment.</summary>
public sealed record PrimaryAdjustmentAssignmentView(string Code, string Title, string RetriesText, string AmountText, string Currency, string Checksum, string RateText, string StatusLabel)
{
    public static PrimaryAdjustmentAssignmentView ToPrimaryAdjustmentAssignmentView(PrimaryAdjustmentAssignment item)
    {
        return new PrimaryAdjustmentAssignmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Checksum,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAdjustmentAssignmentView> ToPrimaryAdjustmentAssignmentViews(List<PrimaryAdjustmentAssignment> items)
    {
        var views = new List<PrimaryAdjustmentAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAdjustmentAssignmentView(item));
        }
        return views;
    }

    public static double SumPrimaryAdjustmentAssignmentView(PrimaryAdjustmentAssignmentView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedAmount + parsedRate;
    }
}
