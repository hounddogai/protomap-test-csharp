namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg audit.</summary>
public sealed record ReorgAuditView(string Code, string Name, string LabelText, string ChecksumText, string Channel, string Discount, string Cost, string BatchSize, string StatusLabel)
{
    public static ReorgAuditView ToReorgAuditView(ReorgAudit item)
    {
        return new ReorgAuditView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Label,
            item.Checksum,
            item.Channel,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgAuditView> ToReorgAuditViews(List<ReorgAudit> items)
    {
        var views = new List<ReorgAuditView>();
        foreach (var item in items)
        {
            views.Add(ToReorgAuditView(item));
        }
        return views;
    }

    public static double SumReorgAuditView(ReorgAuditView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedCost + (double)parsedBatchSize;
    }
}
