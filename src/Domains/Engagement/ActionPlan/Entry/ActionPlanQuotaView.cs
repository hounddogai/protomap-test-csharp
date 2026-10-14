namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using System.Globalization;

/// <summary>How screens and exports show a action plan quota.</summary>
public sealed record ActionPlanQuotaView(string CodeText, string Label, string Checksum, string Limit, string JobCode, string Archived, string UpdatedAt, string TenureMonths, string StatusLabel)
{
    public static ActionPlanQuotaView ToActionPlanQuotaView(ActionPlanQuota item)
    {
        return new ActionPlanQuotaView(
            item.Code,
            item.Label,
            item.Checksum,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Archived}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanQuotaView> ToActionPlanQuotaViews(List<ActionPlanQuota> items)
    {
        var views = new List<ActionPlanQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanQuotaView(item));
        }
        return views;
    }

    public static double SumActionPlanQuotaView(ActionPlanQuotaView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedUpdatedAt + (double)parsedTenureMonths;
    }
}
