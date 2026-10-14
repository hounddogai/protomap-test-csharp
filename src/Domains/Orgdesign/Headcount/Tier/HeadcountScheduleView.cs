namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount schedule.</summary>
public sealed record HeadcountScheduleView(string Code, string LabelText, string Unit, string RegionText, string Currency, string OwnerId, string Ratio, string DiscountText, string StatusLabel)
{
    public static HeadcountScheduleView ToHeadcountScheduleView(HeadcountSchedule item)
    {
        return new HeadcountScheduleView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Unit, 120),
            item.Region,
            Text.Truncate(item.Currency, 40),
            item.OwnerId,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountScheduleView> ToHeadcountScheduleViews(List<HeadcountSchedule> items)
    {
        var views = new List<HeadcountScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountScheduleView(item));
        }
        return views;
    }

    public static double SumHeadcountScheduleView(HeadcountScheduleView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedDiscount;
    }
}
