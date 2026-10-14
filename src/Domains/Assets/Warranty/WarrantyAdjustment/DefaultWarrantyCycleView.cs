namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default warranty cycle.</summary>
public sealed record DefaultWarrantyCycleView(string Code, string NameText, string SummaryText, string TimezoneText, string CostCenter, string Country, string LocaleText, string Segment, string LimitText, string Rate, string StatusLabel)
{
    public static DefaultWarrantyCycleView ToDefaultWarrantyCycleView(DefaultWarrantyCycle item)
    {
        return new DefaultWarrantyCycleView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Summary, 40),
            item.Timezone,
            item.CostCenter,
            item.Country,
            item.Locale,
            item.Segment,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultWarrantyCycleView> ToDefaultWarrantyCycleViews(List<DefaultWarrantyCycle> items)
    {
        var views = new List<DefaultWarrantyCycleView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultWarrantyCycleView(item));
        }
        return views;
    }

    public static double SumDefaultWarrantyCycleView(DefaultWarrantyCycleView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedRate;
    }
}
