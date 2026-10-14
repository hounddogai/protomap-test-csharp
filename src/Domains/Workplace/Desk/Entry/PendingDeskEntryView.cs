namespace Acme.Workforce.Workplace.Desk.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending desk entry.</summary>
public sealed record PendingDeskEntryView(string CodeText, string Label, string Department, string Score, string Pinned, string UnitText, string StatusLabel)
{
    public static PendingDeskEntryView ToPendingDeskEntryView(PendingDeskEntry item)
    {
        return new PendingDeskEntryView(
            item.Code,
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Department, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Unit, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingDeskEntryView> ToPendingDeskEntryViews(List<PendingDeskEntry> items)
    {
        var views = new List<PendingDeskEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPendingDeskEntryView(item));
        }
        return views;
    }

    public static double SumPendingDeskEntryView(PendingDeskEntryView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
