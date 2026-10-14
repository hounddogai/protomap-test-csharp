namespace Acme.Workforce.Engagement.TownHall.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive town hall notice.</summary>
public sealed record AdaptiveTownHallNoticeView(string CodeText, string Title, string UpdatedAtText, string CostCenter, string Segment, string Score, string Budget, string Country, string Locale, string ArchivedText, string Cost, string StatusLabel)
{
    public static AdaptiveTownHallNoticeView ToAdaptiveTownHallNoticeView(AdaptiveTownHallNotice item)
    {
        return new AdaptiveTownHallNoticeView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Segment, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Locale,
            $"{item.Archived}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTownHallNoticeView> ToAdaptiveTownHallNoticeViews(List<AdaptiveTownHallNotice> items)
    {
        var views = new List<AdaptiveTownHallNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTownHallNoticeView(item));
        }
        return views;
    }

    public static double SumAdaptiveTownHallNoticeView(AdaptiveTownHallNoticeView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedScore + parsedBudget;
    }
}
