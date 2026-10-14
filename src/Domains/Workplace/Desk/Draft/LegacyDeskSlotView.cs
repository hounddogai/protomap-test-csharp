namespace Acme.Workforce.Workplace.Desk.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy desk slot.</summary>
public sealed record LegacyDeskSlotView(string Code, string TitleText, string NotesText, string Margin, string VisibleText, string Country, string Phone, string TenureMonthsText, string StatusLabel)
{
    public static LegacyDeskSlotView ToLegacyDeskSlotView(LegacyDeskSlot item)
    {
        return new LegacyDeskSlotView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Notes,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.Country, 40),
            item.Phone,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyDeskSlotView> ToLegacyDeskSlotViews(List<LegacyDeskSlot> items)
    {
        var views = new List<LegacyDeskSlotView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyDeskSlotView(item));
        }
        return views;
    }

    public static double SumLegacyDeskSlotView(LegacyDeskSlotView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedTenureMonths;
    }
}
