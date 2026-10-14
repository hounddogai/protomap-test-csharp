namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount slot.</summary>
public sealed record HeadcountSlotView(string Code, string TitleText, string AmountText, string Maximum, string GraceDays, string DurationMinutesText, string StatusLabel)
{
    public static HeadcountSlotView ToHeadcountSlotView(HeadcountSlot item)
    {
        return new HeadcountSlotView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountSlotView> ToHeadcountSlotViews(List<HeadcountSlot> items)
    {
        var views = new List<HeadcountSlotView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountSlotView(item));
        }
        return views;
    }

    public static double SumHeadcountSlotView(HeadcountSlotView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedAmount + parsedMaximum + (double)parsedGraceDays;
    }
}
