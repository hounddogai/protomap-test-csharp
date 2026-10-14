namespace Acme.Workforce.Orgdesign.Span.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global span slot.</summary>
public sealed record GlobalSpanSlotView(string Code, string Title, string Revision, string Location, string Channel, string Discount, string ManagerId, string EndsAt, string StartsAt, string StatusLabel)
{
    public static GlobalSpanSlotView ToGlobalSpanSlotView(GlobalSpanSlot item)
    {
        return new GlobalSpanSlotView(
            item.Code,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            Text.Truncate(item.Channel, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalSpanSlotView> ToGlobalSpanSlotViews(List<GlobalSpanSlot> items)
    {
        var views = new List<GlobalSpanSlotView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalSpanSlotView(item));
        }
        return views;
    }

    public static double SumGlobalSpanSlotView(GlobalSpanSlotView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedDiscount + (double)parsedEndsAt;
    }
}
