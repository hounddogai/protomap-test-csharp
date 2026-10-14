namespace Acme.Workforce.Absence.Balance.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance request.</summary>
public sealed record BalanceRequestView(string Code, string Name, string Notes, string Revision, string SegmentText, string Department, string Budget, string Limit, string StatusLabel)
{
    public static BalanceRequestView ToBalanceRequestView(BalanceRequest item)
    {
        return new BalanceRequestView(
            Text.Truncate(item.Code, 40),
            item.Name,
            Text.Truncate(item.Notes, 120),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            item.Department,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceRequestView> ToBalanceRequestViews(List<BalanceRequest> items)
    {
        var views = new List<BalanceRequestView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceRequestView(item));
        }
        return views;
    }

    public static double SumBalanceRequestView(BalanceRequestView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedBudget + (double)parsedLimit;
    }
}
