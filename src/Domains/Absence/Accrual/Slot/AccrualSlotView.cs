namespace Acme.Workforce.Absence.Accrual.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual slot.</summary>
public sealed record AccrualSlotView(string Code, string Name, string Locale, string Region, string MinimumText, string StartsAtText, string Phone, string CostCenter, string Cost, string Category, string Limit, string StatusLabel)
{
    public static AccrualSlotView ToAccrualSlotView(AccrualSlot item)
    {
        return new AccrualSlotView(
            item.Code,
            item.Name,
            Text.Truncate(item.Locale, 80),
            item.Region,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 120),
            item.CostCenter,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualSlotView> ToAccrualSlotViews(List<AccrualSlot> items)
    {
        var views = new List<AccrualSlotView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualSlotView(item));
        }
        return views;
    }

    public static double SumAccrualSlotView(AccrualSlotView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedStartsAt + parsedCost;
    }
}
