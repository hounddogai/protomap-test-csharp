namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg request.</summary>
public sealed record ReorgRequestView(string CodeText, string Title, string BatchSizeText, string Ratio, string Discount, string ManagerIdText, string Source, string StatusLabel)
{
    public static ReorgRequestView ToReorgRequestView(ReorgRequest item)
    {
        return new ReorgRequestView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgRequestView> ToReorgRequestViews(List<ReorgRequest> items)
    {
        var views = new List<ReorgRequestView>();
        foreach (var item in items)
        {
            views.Add(ToReorgRequestView(item));
        }
        return views;
    }

    public static double SumReorgRequestView(ReorgRequestView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedRatio + parsedDiscount;
    }
}
