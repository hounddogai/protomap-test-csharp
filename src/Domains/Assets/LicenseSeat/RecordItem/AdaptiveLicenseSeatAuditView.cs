namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive license seat audit.</summary>
public sealed record AdaptiveLicenseSeatAuditView(string Code, string Label, string Segment, string ContactEmail, string RatingText, string TenureMonths, string CostText, string EmployeeIdText, string StatusLabel)
{
    public static AdaptiveLicenseSeatAuditView ToAdaptiveLicenseSeatAuditView(AdaptiveLicenseSeatAudit item)
    {
        return new AdaptiveLicenseSeatAuditView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Segment,
            Text.Truncate(item.ContactEmail, 64),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveLicenseSeatAuditView> ToAdaptiveLicenseSeatAuditViews(List<AdaptiveLicenseSeatAudit> items)
    {
        var views = new List<AdaptiveLicenseSeatAuditView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveLicenseSeatAuditView(item));
        }
        return views;
    }

    public static double SumAdaptiveLicenseSeatAuditView(AdaptiveLicenseSeatAuditView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedTenureMonths + parsedCost;
    }
}
