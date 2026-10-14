namespace Acme.Workforce.Absence.Accrual.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered accrual threshold.</summary>
public sealed record TieredAccrualThresholdView(string Code, string Name, string Ratio, string LimitText, string Segment, string Title, string FteText, string StatusLabel)
{
    public static TieredAccrualThresholdView ToTieredAccrualThresholdView(TieredAccrualThreshold item)
    {
        return new TieredAccrualThresholdView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 80),
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredAccrualThresholdView> ToTieredAccrualThresholdViews(List<TieredAccrualThreshold> items)
    {
        var views = new List<TieredAccrualThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToTieredAccrualThresholdView(item));
        }
        return views;
    }

    public static double SumTieredAccrualThresholdView(TieredAccrualThresholdView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedLimit + parsedFte;
    }
}
