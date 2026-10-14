namespace Acme.Workforce.Talent.Mobility.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled mobility entry.</summary>
public sealed record ScheduledMobilityEntryView(string Code, string Title, string Ratio, string UnitText, string Location, string ManagerId, string ChannelText, string Maximum, string ContactEmail, string StatusLabel)
{
    public static ScheduledMobilityEntryView ToScheduledMobilityEntryView(ScheduledMobilityEntry item)
    {
        return new ScheduledMobilityEntryView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            Text.Truncate(item.Location, 64),
            item.ManagerId,
            Text.Truncate(item.Channel, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledMobilityEntryView> ToScheduledMobilityEntryViews(List<ScheduledMobilityEntry> items)
    {
        var views = new List<ScheduledMobilityEntryView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledMobilityEntryView(item));
        }
        return views;
    }

    public static double SumScheduledMobilityEntryView(ScheduledMobilityEntryView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedRatio + parsedMaximum;
    }
}
