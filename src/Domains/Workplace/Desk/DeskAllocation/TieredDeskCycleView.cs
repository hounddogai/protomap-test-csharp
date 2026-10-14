namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered desk cycle.</summary>
public sealed record TieredDeskCycleView(string Code, string Label, string Description, string ContactEmail, string OwnerIdText, string Phone, string Retries, string Locale, string Timezone, string StatusLabel)
{
    public static TieredDeskCycleView ToTieredDeskCycleView(TieredDeskCycle item)
    {
        return new TieredDeskCycleView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Description, 120),
            item.ContactEmail,
            item.OwnerId,
            item.Phone,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredDeskCycleView> ToTieredDeskCycleViews(List<TieredDeskCycle> items)
    {
        var views = new List<TieredDeskCycleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredDeskCycleView(item));
        }
        return views;
    }

    public static double SumTieredDeskCycleView(TieredDeskCycleView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
