namespace Acme.Workforce.Engagement.TownHall.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive town hall ledger.</summary>
public sealed record AdaptiveTownHallLedgerView(string Code, string Name, string ManagerId, string ApprovedText, string Score, string Title, string Summary, string Reviewer, string StatusLabel)
{
    public static AdaptiveTownHallLedgerView ToAdaptiveTownHallLedgerView(AdaptiveTownHallLedger item)
    {
        return new AdaptiveTownHallLedgerView(
            item.Code,
            item.Name,
            Text.Truncate(item.ManagerId, 64),
            $"{item.Approved}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 120),
            item.Summary,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTownHallLedgerView> ToAdaptiveTownHallLedgerViews(List<AdaptiveTownHallLedger> items)
    {
        var views = new List<AdaptiveTownHallLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTownHallLedgerView(item));
        }
        return views;
    }

    public static double SumAdaptiveTownHallLedgerView(AdaptiveTownHallLedgerView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
