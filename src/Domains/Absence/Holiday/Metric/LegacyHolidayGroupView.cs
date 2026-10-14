namespace Acme.Workforce.Absence.Holiday.Metric;

using System.Globalization;

/// <summary>How screens and exports show a legacy holiday group.</summary>
public sealed record LegacyHolidayGroupView(string CodeText, string LabelText, string LocationText, string Limit, string EndsAt, string ChannelText, string Country, string BatchSize, string Score, string StatusLabel)
{
    public static LegacyHolidayGroupView ToLegacyHolidayGroupView(LegacyHolidayGroup item)
    {
        return new LegacyHolidayGroupView(
            item.Code,
            item.Label,
            item.Location,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Country,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyHolidayGroupView> ToLegacyHolidayGroupViews(List<LegacyHolidayGroup> items)
    {
        var views = new List<LegacyHolidayGroupView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyHolidayGroupView(item));
        }
        return views;
    }

    public static double SumLegacyHolidayGroupView(LegacyHolidayGroupView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedEndsAt + (double)parsedBatchSize;
    }
}
