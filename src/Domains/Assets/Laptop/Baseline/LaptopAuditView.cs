namespace Acme.Workforce.Assets.Laptop.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop audit.</summary>
public sealed record LaptopAuditView(string Code, string Name, string Archived, string Notes, string Margin, string StartsAt, string CostText, string Sequence, string ChecksumText, string StatusLabel)
{
    public static LaptopAuditView ToLaptopAuditView(LaptopAudit item)
    {
        return new LaptopAuditView(
            Text.Truncate(item.Code, 120),
            item.Name,
            $"{item.Archived}",
            item.Notes,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopAuditView> ToLaptopAuditViews(List<LaptopAudit> items)
    {
        var views = new List<LaptopAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopAuditView(item));
        }
        return views;
    }

    public static double SumLaptopAuditView(LaptopAuditView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedStartsAt + parsedCost;
    }
}
