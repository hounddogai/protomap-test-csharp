namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending visitor slot.</summary>
public sealed record PendingVisitorSlotView(string Code, string Title, string Timezone, string DurationMinutes, string GraceDays, string StatusLabel)
{
    public static PendingVisitorSlotView ToPendingVisitorSlotView(PendingVisitorSlot item)
    {
        return new PendingVisitorSlotView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.Timezone,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingVisitorSlotView> ToPendingVisitorSlotViews(List<PendingVisitorSlot> items)
    {
        var views = new List<PendingVisitorSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPendingVisitorSlotView(item));
        }
        return views;
    }

    public static double SumPendingVisitorSlotView(PendingVisitorSlotView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedGraceDays;
    }
}
