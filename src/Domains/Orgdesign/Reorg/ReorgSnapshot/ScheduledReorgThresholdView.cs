namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled reorg threshold.</summary>
public sealed record ScheduledReorgThresholdView(string Code, string LabelText, string EndsAtText, string Quantity, string Title, string StatusLabel)
{
    public static ScheduledReorgThresholdView ToScheduledReorgThresholdView(ScheduledReorgThreshold item)
    {
        return new ScheduledReorgThresholdView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledReorgThresholdView> ToScheduledReorgThresholdViews(List<ScheduledReorgThreshold> items)
    {
        var views = new List<ScheduledReorgThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledReorgThresholdView(item));
        }
        return views;
    }

    public static double SumScheduledReorgThresholdView(ScheduledReorgThresholdView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedQuantity;
    }
}
