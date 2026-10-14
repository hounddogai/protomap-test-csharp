namespace Acme.Workforce.Assets.Refresh.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled refresh cycle.</summary>
public sealed record ScheduledRefreshCycleView(string CodeText, string Label, string DurationMinutesText, string Channel, string AmountText, string Fte, string Ceiling, string StatusLabel)
{
    public static ScheduledRefreshCycleView ToScheduledRefreshCycleView(ScheduledRefreshCycle item)
    {
        return new ScheduledRefreshCycleView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRefreshCycleView> ToScheduledRefreshCycleViews(List<ScheduledRefreshCycle> items)
    {
        var views = new List<ScheduledRefreshCycleView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRefreshCycleView(item));
        }
        return views;
    }

    public static double SumScheduledRefreshCycleView(ScheduledRefreshCycleView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedAmount + parsedFte;
    }
}
