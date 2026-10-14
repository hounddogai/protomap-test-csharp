namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg rollup.</summary>
public sealed record ReorgRollupView(string Code, string NameText, string Summary, string Hours, string CeilingText, string StatusLabel)
{
    public static ReorgRollupView ToReorgRollupView(ReorgRollup item)
    {
        return new ReorgRollupView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Summary, 64),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgRollupView> ToReorgRollupViews(List<ReorgRollup> items)
    {
        var views = new List<ReorgRollupView>();
        foreach (var item in items)
        {
            views.Add(ToReorgRollupView(item));
        }
        return views;
    }

    public static double SumReorgRollupView(ReorgRollupView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedHours + parsedCeiling;
    }
}
