namespace Acme.Workforce.Compensation.BonusPool.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool entry.</summary>
public sealed record BonusPoolEntryView(string Code, string NameText, string Summary, string NotesText, string TenureMonths, string Priority, string Active, string StatusLabel)
{
    public static BonusPoolEntryView ToBonusPoolEntryView(BonusPoolEntry item)
    {
        return new BonusPoolEntryView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.Summary, 40),
            item.Notes,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolEntryView> ToBonusPoolEntryViews(List<BonusPoolEntry> items)
    {
        var views = new List<BonusPoolEntryView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolEntryView(item));
        }
        return views;
    }

    public static double SumBonusPoolEntryView(BonusPoolEntryView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedPriority;
    }
}
