namespace Acme.Workforce.Workplace.Visitor.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted visitor entry.</summary>
public sealed record WeightedVisitorEntryView(string Code, string NameText, string Badge, string Phone, string Locale, string Timezone, string WindowDays, string ApprovedText, string StatusLabel)
{
    public static WeightedVisitorEntryView ToWeightedVisitorEntryView(WeightedVisitorEntry item)
    {
        return new WeightedVisitorEntryView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Badge,
            item.Phone,
            item.Locale,
            item.Timezone,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedVisitorEntryView> ToWeightedVisitorEntryViews(List<WeightedVisitorEntry> items)
    {
        var views = new List<WeightedVisitorEntryView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedVisitorEntryView(item));
        }
        return views;
    }

    public static double SumWeightedVisitorEntryView(WeightedVisitorEntryView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}
