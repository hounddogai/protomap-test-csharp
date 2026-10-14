namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending bonus pool allocation.</summary>
public sealed record PendingBonusPoolAllocationView(string CodeText, string Name, string ExpiresAt, string Source, string NotesText, string Pinned, string Badge, string DurationMinutesText, string StatusLabel)
{
    public static PendingBonusPoolAllocationView ToPendingBonusPoolAllocationView(PendingBonusPoolAllocation item)
    {
        return new PendingBonusPoolAllocationView(
            item.Code,
            item.Name,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 120),
            item.Notes,
            $"{item.Pinned}",
            Text.Truncate(item.Badge, 80),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingBonusPoolAllocationView> ToPendingBonusPoolAllocationViews(List<PendingBonusPoolAllocation> items)
    {
        var views = new List<PendingBonusPoolAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPendingBonusPoolAllocationView(item));
        }
        return views;
    }

    public static double SumPendingBonusPoolAllocationView(PendingBonusPoolAllocationView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedDurationMinutes;
    }
}
