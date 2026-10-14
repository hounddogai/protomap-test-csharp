namespace Acme.Workforce.Compensation.Adjustment.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment template.</summary>
public sealed record AdjustmentTemplateView(string Code, string Name, string TenureMonthsText, string LimitText, string StatusLabel)
{
    public static AdjustmentTemplateView ToAdjustmentTemplateView(AdjustmentTemplate item)
    {
        return new AdjustmentTemplateView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentTemplateView> ToAdjustmentTemplateViews(List<AdjustmentTemplate> items)
    {
        var views = new List<AdjustmentTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentTemplateView(item));
        }
        return views;
    }

    public static double SumAdjustmentTemplateView(AdjustmentTemplateView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedLimit;
    }
}
