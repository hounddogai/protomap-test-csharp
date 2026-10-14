namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan quota.</summary>
public sealed record ReturnPlanQuotaView(string Code, string NameText, string Capacity, string StartsAt, string EndsAt, string RevisionText, string TenureMonths, string Minimum, string CostCenterText, string FloorValue, string StatusLabel)
{
    public static ReturnPlanQuotaView ToReturnPlanQuotaView(ReturnPlanQuota item)
    {
        return new ReturnPlanQuotaView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanQuotaView> ToReturnPlanQuotaViews(List<ReturnPlanQuota> items)
    {
        var views = new List<ReturnPlanQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanQuotaView(item));
        }
        return views;
    }

    public static double SumReturnPlanQuotaView(ReturnPlanQuotaView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedStartsAt + (double)parsedEndsAt;
    }
}
