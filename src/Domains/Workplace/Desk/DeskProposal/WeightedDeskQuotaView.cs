namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted desk quota.</summary>
public sealed record WeightedDeskQuotaView(string Code, string Label, string SalaryBandText, string TitleText, string CostCenter, string PinnedText, string Fte, string EmployeeId, string BatchSizeText, string StatusLabel)
{
    public static WeightedDeskQuotaView ToWeightedDeskQuotaView(WeightedDeskQuota item)
    {
        return new WeightedDeskQuotaView(
            item.Code,
            item.Label,
            item.SalaryBand,
            item.Title,
            Text.Truncate(item.CostCenter, 64),
            $"{item.Pinned}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedDeskQuotaView> ToWeightedDeskQuotaViews(List<WeightedDeskQuota> items)
    {
        var views = new List<WeightedDeskQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedDeskQuotaView(item));
        }
        return views;
    }

    public static double SumWeightedDeskQuotaView(WeightedDeskQuotaView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedBatchSize;
    }
}
