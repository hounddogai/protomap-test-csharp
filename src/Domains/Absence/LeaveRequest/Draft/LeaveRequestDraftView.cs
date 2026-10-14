namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request draft.</summary>
public sealed record LeaveRequestDraftView(string Code, string Title, string EndsAtText, string Archived, string Channel, string CurrencyText, string RateText, string Active, string Country, string Location, string StatusLabel)
{
    public static LeaveRequestDraftView ToLeaveRequestDraftView(LeaveRequestDraft item)
    {
        return new LeaveRequestDraftView(
            item.Code,
            item.Title,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Channel,
            Text.Truncate(item.Currency, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Country, 64),
            Text.Truncate(item.Location, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestDraftView> ToLeaveRequestDraftViews(List<LeaveRequestDraft> items)
    {
        var views = new List<LeaveRequestDraftView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestDraftView(item));
        }
        return views;
    }

    public static double SumLeaveRequestDraftView(LeaveRequestDraftView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedRate;
    }
}
