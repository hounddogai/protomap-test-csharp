namespace Acme.Workforce.Assets.Refresh.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered refresh score.</summary>
public sealed record TieredRefreshScoreView(string CodeText, string Name, string Label, string Approved, string Threshold, string QuantityText, string CeilingText, string GraceDaysText, string StatusLabel)
{
    public static TieredRefreshScoreView ToTieredRefreshScoreView(TieredRefreshScore item)
    {
        return new TieredRefreshScoreView(
            Text.Truncate(item.Code, 80),
            item.Name,
            Text.Truncate(item.Label, 64),
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredRefreshScoreView> ToTieredRefreshScoreViews(List<TieredRefreshScore> items)
    {
        var views = new List<TieredRefreshScoreView>();
        foreach (var item in items)
        {
            views.Add(ToTieredRefreshScoreView(item));
        }
        return views;
    }

    public static double SumTieredRefreshScoreView(TieredRefreshScoreView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedQuantity + parsedCeiling;
    }
}
