namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending reorg variance.</summary>
public sealed record PendingReorgVarianceView(string Code, string Name, string Quantity, string Description, string Budget, string Approved, string DurationMinutesText, string Threshold, string StatusLabel)
{
    public static PendingReorgVarianceView ToPendingReorgVarianceView(PendingReorgVariance item)
    {
        return new PendingReorgVarianceView(
            item.Code,
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 80),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingReorgVarianceView> ToPendingReorgVarianceViews(List<PendingReorgVariance> items)
    {
        var views = new List<PendingReorgVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToPendingReorgVarianceView(item));
        }
        return views;
    }

    public static double SumPendingReorgVarianceView(PendingReorgVarianceView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedBudget + (double)parsedDurationMinutes;
    }
}
