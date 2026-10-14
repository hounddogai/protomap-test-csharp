namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form slot.</summary>
public sealed record TaxFormSlotView(string Code, string Title, string CeilingText, string Retries, string TenureMonthsText, string ActiveText, string BadgeText, string StatusLabel)
{
    public static TaxFormSlotView ToTaxFormSlotView(TaxFormSlot item)
    {
        return new TaxFormSlotView(
            item.Code,
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Badge, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormSlotView> ToTaxFormSlotViews(List<TaxFormSlot> items)
    {
        var views = new List<TaxFormSlotView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormSlotView(item));
        }
        return views;
    }

    public static double SumTaxFormSlotView(TaxFormSlotView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedRetries + (double)parsedTenureMonths;
    }
}
