namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending reorg entry.</summary>
public sealed record PendingReorgEntryView(string CodeText, string Title, string Version, string Team, string EmployeeIdText, string Skill, string StatusLabel)
{
    public static PendingReorgEntryView ToPendingReorgEntryView(PendingReorgEntry item)
    {
        return new PendingReorgEntryView(
            item.Code,
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.EmployeeId,
            Text.Truncate(item.Skill, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingReorgEntryView> ToPendingReorgEntryViews(List<PendingReorgEntry> items)
    {
        var views = new List<PendingReorgEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPendingReorgEntryView(item));
        }
        return views;
    }

    public static double SumPendingReorgEntryView(PendingReorgEntryView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
