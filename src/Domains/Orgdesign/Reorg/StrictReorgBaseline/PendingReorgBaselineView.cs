namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending reorg baseline.</summary>
public sealed record PendingReorgBaselineView(string CodeText, string LabelText, string ContactEmail, string EndsAtText, string GraceDays, string BadgeText, string PriorityText, string QuantityText, string StatusLabel)
{
    public static PendingReorgBaselineView ToPendingReorgBaselineView(PendingReorgBaseline item)
    {
        return new PendingReorgBaselineView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.ContactEmail, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingReorgBaselineView> ToPendingReorgBaselineViews(List<PendingReorgBaseline> items)
    {
        var views = new List<PendingReorgBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPendingReorgBaselineView(item));
        }
        return views;
    }

    public static double SumPendingReorgBaselineView(PendingReorgBaselineView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedGraceDays + (double)parsedPriority;
    }
}
