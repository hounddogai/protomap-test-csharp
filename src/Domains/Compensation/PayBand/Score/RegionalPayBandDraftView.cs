namespace Acme.Workforce.Compensation.PayBand.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional pay band draft.</summary>
public sealed record RegionalPayBandDraftView(string CodeText, string TitleText, string Notes, string Phone, string LeaveDaysText, string WindowDaysText, string Quantity, string Archived, string StartsAtText, string StatusLabel)
{
    public static RegionalPayBandDraftView ToRegionalPayBandDraftView(RegionalPayBandDraft item)
    {
        return new RegionalPayBandDraftView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.Notes,
            item.Phone,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPayBandDraftView> ToRegionalPayBandDraftViews(List<RegionalPayBandDraft> items)
    {
        var views = new List<RegionalPayBandDraftView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPayBandDraftView(item));
        }
        return views;
    }

    public static double SumRegionalPayBandDraftView(RegionalPayBandDraftView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedWindowDays + (double)parsedQuantity;
    }
}
