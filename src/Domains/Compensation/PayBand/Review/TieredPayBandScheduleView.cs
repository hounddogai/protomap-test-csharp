namespace Acme.Workforce.Compensation.PayBand.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered pay band schedule.</summary>
public sealed record TieredPayBandScheduleView(string Code, string Name, string ExternalRefText, string Title, string EmployeeId, string Limit, string Margin, string Timezone, string Source, string Revision, string StatusLabel)
{
    public static TieredPayBandScheduleView ToTieredPayBandScheduleView(TieredPayBandSchedule item)
    {
        return new TieredPayBandScheduleView(
            item.Code,
            item.Name,
            Text.Truncate(item.ExternalRef, 120),
            item.Title,
            item.EmployeeId,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 40),
            item.Source,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPayBandScheduleView> ToTieredPayBandScheduleViews(List<TieredPayBandSchedule> items)
    {
        var views = new List<TieredPayBandScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPayBandScheduleView(item));
        }
        return views;
    }

    public static double SumTieredPayBandScheduleView(TieredPayBandScheduleView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMargin + (double)parsedRevision;
    }
}
