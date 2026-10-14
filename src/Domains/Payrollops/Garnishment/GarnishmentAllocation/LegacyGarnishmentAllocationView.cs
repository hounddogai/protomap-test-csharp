namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy garnishment allocation.</summary>
public sealed record LegacyGarnishmentAllocationView(string CodeText, string Label, string Margin, string SalaryBand, string EmployeeId, string Phone, string DurationMinutes, string PinnedText, string StatusLabel)
{
    public static LegacyGarnishmentAllocationView ToLegacyGarnishmentAllocationView(LegacyGarnishmentAllocation item)
    {
        return new LegacyGarnishmentAllocationView(
            item.Code,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            Text.Truncate(item.EmployeeId, 80),
            Text.Truncate(item.Phone, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyGarnishmentAllocationView> ToLegacyGarnishmentAllocationViews(List<LegacyGarnishmentAllocation> items)
    {
        var views = new List<LegacyGarnishmentAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyGarnishmentAllocationView(item));
        }
        return views;
    }

    public static double SumLegacyGarnishmentAllocationView(LegacyGarnishmentAllocationView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedDurationMinutes;
    }
}
