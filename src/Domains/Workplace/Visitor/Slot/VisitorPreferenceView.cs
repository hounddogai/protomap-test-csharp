namespace Acme.Workforce.Workplace.Visitor.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor preference.</summary>
public sealed record VisitorPreferenceView(string Code, string Title, string Priority, string TenureMonths, string CapacityText, string Weight, string Description, string Phone, string Pinned, string SummaryText, string SalaryBand, string StatusLabel)
{
    public static VisitorPreferenceView ToVisitorPreferenceView(VisitorPreference item)
    {
        return new VisitorPreferenceView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Phone, 80),
            $"{item.Pinned}",
            item.Summary,
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorPreferenceView> ToVisitorPreferenceViews(List<VisitorPreference> items)
    {
        var views = new List<VisitorPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorPreferenceView(item));
        }
        return views;
    }

    public static double SumVisitorPreferenceView(VisitorPreferenceView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedTenureMonths + (double)parsedCapacity;
    }
}
