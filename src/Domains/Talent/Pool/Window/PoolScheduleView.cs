namespace Acme.Workforce.Talent.Pool.Window;

using System.Globalization;

/// <summary>How screens and exports show a pool schedule.</summary>
public sealed record PoolScheduleView(string Code, string Title, string ArchivedText, string Department, string Phone, string Category, string BadgeText, string EndsAtText, string StatusLabel)
{
    public static PoolScheduleView ToPoolScheduleView(PoolSchedule item)
    {
        return new PoolScheduleView(
            item.Code,
            item.Title,
            $"{item.Archived}",
            item.Department,
            item.Phone,
            item.Category,
            item.Badge,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolScheduleView> ToPoolScheduleViews(List<PoolSchedule> items)
    {
        var views = new List<PoolScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToPoolScheduleView(item));
        }
        return views;
    }

    public static double SumPoolScheduleView(PoolScheduleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
