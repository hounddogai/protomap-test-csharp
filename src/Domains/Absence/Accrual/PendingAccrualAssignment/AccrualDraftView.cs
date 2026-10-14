namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual draft.</summary>
public sealed record AccrualDraftView(string CodeText, string LabelText, string Country, string Archived, string EndsAt, string StatusLabel)
{
    public static AccrualDraftView ToAccrualDraftView(AccrualDraft item)
    {
        return new AccrualDraftView(
            item.Code,
            item.Label,
            Text.Truncate(item.Country, 64),
            $"{item.Archived}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualDraftView> ToAccrualDraftViews(List<AccrualDraft> items)
    {
        var views = new List<AccrualDraftView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualDraftView(item));
        }
        return views;
    }

    public static double SumAccrualDraftView(AccrualDraftView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
