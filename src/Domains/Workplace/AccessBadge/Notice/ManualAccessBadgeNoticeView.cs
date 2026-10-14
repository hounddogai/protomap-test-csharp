namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual access badge notice.</summary>
public sealed record ManualAccessBadgeNoticeView(string Code, string Name, string SalaryBand, string PriorityText, string Rating, string CostCenterText, string StatusLabel)
{
    public static ManualAccessBadgeNoticeView ToManualAccessBadgeNoticeView(ManualAccessBadgeNotice item)
    {
        return new ManualAccessBadgeNoticeView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.SalaryBand,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAccessBadgeNoticeView> ToManualAccessBadgeNoticeViews(List<ManualAccessBadgeNotice> items)
    {
        var views = new List<ManualAccessBadgeNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToManualAccessBadgeNoticeView(item));
        }
        return views;
    }

    public static double SumManualAccessBadgeNoticeView(ManualAccessBadgeNoticeView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedRating;
    }
}
