namespace Acme.Workforce.Assets.Laptop.Slot;

using System.Globalization;

/// <summary>How screens and exports show a global laptop slot.</summary>
public sealed record GlobalLaptopSlotView(string Code, string Name, string OwnerIdText, string TenureMonthsText, string Active, string CostCenter, string AmountText, string StatusLabel)
{
    public static GlobalLaptopSlotView ToGlobalLaptopSlotView(GlobalLaptopSlot item)
    {
        return new GlobalLaptopSlotView(
            item.Code,
            item.Name,
            item.OwnerId,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.CostCenter,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalLaptopSlotView> ToGlobalLaptopSlotViews(List<GlobalLaptopSlot> items)
    {
        var views = new List<GlobalLaptopSlotView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalLaptopSlotView(item));
        }
        return views;
    }

    public static double SumGlobalLaptopSlotView(GlobalLaptopSlotView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedAmount;
    }
}
