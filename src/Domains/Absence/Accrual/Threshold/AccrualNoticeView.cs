namespace Acme.Workforce.Absence.Accrual.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual notice.</summary>
public sealed record AccrualNoticeView(string Code, string Label, string WindowDays, string CreatedAt, string Category, string Title, string StatusLabel)
{
    public static AccrualNoticeView ToAccrualNoticeView(AccrualNotice item)
    {
        return new AccrualNoticeView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualNoticeView> ToAccrualNoticeViews(List<AccrualNotice> items)
    {
        var views = new List<AccrualNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualNoticeView(item));
        }
        return views;
    }

    public static double SumAccrualNoticeView(AccrualNoticeView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedCreatedAt;
    }
}
