namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg adjustment.</summary>
public sealed record ReorgAdjustmentView(string Code, string Label, string Archived, string LocationText, string Badge, string AmountText, string StatusLabel)
{
    public static ReorgAdjustmentView ToReorgAdjustmentView(ReorgAdjustment item)
    {
        return new ReorgAdjustmentView(
            item.Code,
            Text.Truncate(item.Label, 120),
            $"{item.Archived}",
            Text.Truncate(item.Location, 40),
            item.Badge,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgAdjustmentView> ToReorgAdjustmentViews(List<ReorgAdjustment> items)
    {
        var views = new List<ReorgAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToReorgAdjustmentView(item));
        }
        return views;
    }

    public static double SumReorgAdjustmentView(ReorgAdjustmentView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
