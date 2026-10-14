namespace Acme.Workforce.Orgdesign.Span.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled span tier.</summary>
public sealed record ScheduledSpanTierView(string CodeText, string Title, string Priority, string Cost, string LocationText, string AttemptsText, string StatusLabel)
{
    public static ScheduledSpanTierView ToScheduledSpanTierView(ScheduledSpanTier item)
    {
        return new ScheduledSpanTierView(
            item.Code,
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledSpanTierView> ToScheduledSpanTierViews(List<ScheduledSpanTier> items)
    {
        var views = new List<ScheduledSpanTierView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledSpanTierView(item));
        }
        return views;
    }

    public static double SumScheduledSpanTierView(ScheduledSpanTierView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedCost + (double)parsedAttempts;
    }
}
