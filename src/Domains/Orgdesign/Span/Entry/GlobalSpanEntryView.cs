namespace Acme.Workforce.Orgdesign.Span.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global span entry.</summary>
public sealed record GlobalSpanEntryView(string CodeText, string NameText, string FloorValue, string Attempts, string DiscountText, string Description, string Fte, string SequenceText, string Region, string StatusLabel)
{
    public static GlobalSpanEntryView ToGlobalSpanEntryView(GlobalSpanEntry item)
    {
        return new GlobalSpanEntryView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalSpanEntryView> ToGlobalSpanEntryViews(List<GlobalSpanEntry> items)
    {
        var views = new List<GlobalSpanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalSpanEntryView(item));
        }
        return views;
    }

    public static double SumGlobalSpanEntryView(GlobalSpanEntryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedAttempts + parsedDiscount;
    }
}
