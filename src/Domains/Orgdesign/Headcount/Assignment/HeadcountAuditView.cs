namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount audit.</summary>
public sealed record HeadcountAuditView(string CodeText, string Label, string CostCenter, string Badge, string AmountText, string Channel, string Score, string ManagerId, string Country, string OwnerId, string Title, string StatusLabel)
{
    public static HeadcountAuditView ToHeadcountAuditView(HeadcountAudit item)
    {
        return new HeadcountAuditView(
            item.Code,
            item.Label,
            item.CostCenter,
            item.Badge,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Country,
            Text.Truncate(item.OwnerId, 64),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountAuditView> ToHeadcountAuditViews(List<HeadcountAudit> items)
    {
        var views = new List<HeadcountAuditView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountAuditView(item));
        }
        return views;
    }

    public static double SumHeadcountAuditView(HeadcountAuditView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedAmount + parsedScore;
    }
}
