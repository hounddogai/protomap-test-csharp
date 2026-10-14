namespace Acme.Workforce.Contractors.Rate.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate slot.</summary>
public sealed record RateSlotView(string Code, string TitleText, string Locale, string FloorValue, string SkillText, string JobCode, string Grade, string GraceDays, string Department, string CategoryText, string Currency, string StatusLabel)
{
    public static RateSlotView ToRateSlotView(RateSlot item)
    {
        return new RateSlotView(
            item.Code,
            item.Title,
            item.Locale,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.JobCode, 64),
            item.Grade,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            item.Category,
            Text.Truncate(item.Currency, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateSlotView> ToRateSlotViews(List<RateSlot> items)
    {
        var views = new List<RateSlotView>();
        foreach (var item in items)
        {
            views.Add(ToRateSlotView(item));
        }
        return views;
    }

    public static double SumRateSlotView(RateSlotView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedGraceDays;
    }
}
