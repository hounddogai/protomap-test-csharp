namespace Acme.Workforce.Talent.Mobility.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled mobility rollup.</summary>
public sealed record ScheduledMobilityRollupView(string CodeText, string Title, string Approved, string Visible, string Phone, string ManagerId, string Pinned, string Quantity, string StatusLabel)
{
    public static ScheduledMobilityRollupView ToScheduledMobilityRollupView(ScheduledMobilityRollup item)
    {
        return new ScheduledMobilityRollupView(
            item.Code,
            Text.Truncate(item.Title, 120),
            $"{item.Approved}",
            $"{item.Visible}",
            item.Phone,
            item.ManagerId,
            $"{item.Pinned}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledMobilityRollupView> ToScheduledMobilityRollupViews(List<ScheduledMobilityRollup> items)
    {
        var views = new List<ScheduledMobilityRollupView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledMobilityRollupView(item));
        }
        return views;
    }

    public static double SumScheduledMobilityRollupView(ScheduledMobilityRollupView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
