namespace Acme.Workforce.Workplace.Occupancy.Batch;

using System.Globalization;

/// <summary>How screens and exports show a occupancy notice.</summary>
public sealed record OccupancyNoticeView(string Code, string Name, string GraceDays, string Department, string Cost, string Skill, string StatusLabel)
{
    public static OccupancyNoticeView ToOccupancyNoticeView(OccupancyNotice item)
    {
        return new OccupancyNoticeView(
            item.Code,
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyNoticeView> ToOccupancyNoticeViews(List<OccupancyNotice> items)
    {
        var views = new List<OccupancyNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyNoticeView(item));
        }
        return views;
    }

    public static double SumOccupancyNoticeView(OccupancyNoticeView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedCost;
    }
}
