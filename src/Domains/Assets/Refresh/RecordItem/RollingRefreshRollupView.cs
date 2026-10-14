namespace Acme.Workforce.Assets.Refresh.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling refresh rollup.</summary>
public sealed record RollingRefreshRollupView(string CodeText, string Title, string Unit, string Ratio, string StatusLabel)
{
    public static RollingRefreshRollupView ToRollingRefreshRollupView(RollingRefreshRollup item)
    {
        return new RollingRefreshRollupView(
            item.Code,
            item.Title,
            Text.Truncate(item.Unit, 40),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRefreshRollupView> ToRollingRefreshRollupViews(List<RollingRefreshRollup> items)
    {
        var views = new List<RollingRefreshRollupView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRefreshRollupView(item));
        }
        return views;
    }

    public static double SumRollingRefreshRollupView(RollingRefreshRollupView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
