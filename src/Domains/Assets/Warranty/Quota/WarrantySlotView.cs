namespace Acme.Workforce.Assets.Warranty.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty slot.</summary>
public sealed record WarrantySlotView(string CodeText, string Label, string Archived, string ExternalRef, string LocationText, string DurationMinutesText, string Limit, string StatusLabel)
{
    public static WarrantySlotView ToWarrantySlotView(WarrantySlot item)
    {
        return new WarrantySlotView(
            item.Code,
            item.Label,
            $"{item.Archived}",
            item.ExternalRef,
            Text.Truncate(item.Location, 120),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantySlotView> ToWarrantySlotViews(List<WarrantySlot> items)
    {
        var views = new List<WarrantySlotView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantySlotView(item));
        }
        return views;
    }

    public static double SumWarrantySlotView(WarrantySlotView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedLimit;
    }
}
