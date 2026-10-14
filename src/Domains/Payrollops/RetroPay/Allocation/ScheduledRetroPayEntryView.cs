namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a scheduled retro pay entry.</summary>
public sealed record ScheduledRetroPayEntryView(string Code, string Title, string BadgeText, string Ratio, string OwnerId, string LeaveDays, string StatusLabel)
{
    public static ScheduledRetroPayEntryView ToScheduledRetroPayEntryView(ScheduledRetroPayEntry item)
    {
        return new ScheduledRetroPayEntryView(
            item.Code,
            item.Title,
            item.Badge,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRetroPayEntryView> ToScheduledRetroPayEntryViews(List<ScheduledRetroPayEntry> items)
    {
        var views = new List<ScheduledRetroPayEntryView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRetroPayEntryView(item));
        }
        return views;
    }

    public static double SumScheduledRetroPayEntryView(ScheduledRetroPayEntryView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedLeaveDays;
    }
}
