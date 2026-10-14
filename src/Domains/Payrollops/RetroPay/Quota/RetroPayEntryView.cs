namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay entry.</summary>
public sealed record RetroPayEntryView(string CodeText, string Name, string Quantity, string DurationMinutes, string Location, string BatchSize, string Label, string StatusLabel)
{
    public static RetroPayEntryView ToRetroPayEntryView(RetroPayEntry item)
    {
        return new RetroPayEntryView(
            item.Code,
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayEntryView> ToRetroPayEntryViews(List<RetroPayEntry> items)
    {
        var views = new List<RetroPayEntryView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayEntryView(item));
        }
        return views;
    }

    public static double SumRetroPayEntryView(RetroPayEntryView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedDurationMinutes + (double)parsedBatchSize;
    }
}
