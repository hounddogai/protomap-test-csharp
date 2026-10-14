namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default headcount quota.</summary>
public sealed record DefaultHeadcountQuotaView(string Code, string Name, string BatchSize, string Ratio, string DurationMinutesText, string GraceDays, string Revision, string Country, string StatusLabel)
{
    public static DefaultHeadcountQuotaView ToDefaultHeadcountQuotaView(DefaultHeadcountQuota item)
    {
        return new DefaultHeadcountQuotaView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultHeadcountQuotaView> ToDefaultHeadcountQuotaViews(List<DefaultHeadcountQuota> items)
    {
        var views = new List<DefaultHeadcountQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultHeadcountQuotaView(item));
        }
        return views;
    }

    public static double SumDefaultHeadcountQuotaView(DefaultHeadcountQuotaView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedRatio + (double)parsedDurationMinutes;
    }
}
