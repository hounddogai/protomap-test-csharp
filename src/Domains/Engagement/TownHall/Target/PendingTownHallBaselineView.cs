namespace Acme.Workforce.Engagement.TownHall.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending town hall baseline.</summary>
public sealed record PendingTownHallBaselineView(string CodeText, string Title, string PriorityText, string Sequence, string Channel, string TeamText, string Location, string MarginText, string Discount, string StatusLabel)
{
    public static PendingTownHallBaselineView ToPendingTownHallBaselineView(PendingTownHallBaseline item)
    {
        return new PendingTownHallBaselineView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            Text.Truncate(item.Team, 120),
            item.Location,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingTownHallBaselineView> ToPendingTownHallBaselineViews(List<PendingTownHallBaseline> items)
    {
        var views = new List<PendingTownHallBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPendingTownHallBaselineView(item));
        }
        return views;
    }

    public static double SumPendingTownHallBaselineView(PendingTownHallBaselineView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedSequence + parsedMargin;
    }
}
