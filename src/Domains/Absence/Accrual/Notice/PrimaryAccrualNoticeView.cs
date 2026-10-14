namespace Acme.Workforce.Absence.Accrual.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary accrual notice.</summary>
public sealed record PrimaryAccrualNoticeView(string Code, string Name, string Archived, string Country, string StartsAt, string StatusLabel)
{
    public static PrimaryAccrualNoticeView ToPrimaryAccrualNoticeView(PrimaryAccrualNotice item)
    {
        return new PrimaryAccrualNoticeView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Archived}",
            Text.Truncate(item.Country, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAccrualNoticeView> ToPrimaryAccrualNoticeViews(List<PrimaryAccrualNotice> items)
    {
        var views = new List<PrimaryAccrualNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAccrualNoticeView(item));
        }
        return views;
    }

    public static double SumPrimaryAccrualNoticeView(PrimaryAccrualNoticeView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
