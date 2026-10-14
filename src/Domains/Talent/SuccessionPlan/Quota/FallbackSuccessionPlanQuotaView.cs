namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback succession plan quota.</summary>
public sealed record FallbackSuccessionPlanQuotaView(string Code, string Name, string NotesText, string Capacity, string Pinned, string SalaryBand, string EndsAtText, string ApprovedText, string StatusLabel)
{
    public static FallbackSuccessionPlanQuotaView ToFallbackSuccessionPlanQuotaView(FallbackSuccessionPlanQuota item)
    {
        return new FallbackSuccessionPlanQuotaView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Notes,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.SalaryBand,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackSuccessionPlanQuotaView> ToFallbackSuccessionPlanQuotaViews(List<FallbackSuccessionPlanQuota> items)
    {
        var views = new List<FallbackSuccessionPlanQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackSuccessionPlanQuotaView(item));
        }
        return views;
    }

    public static double SumFallbackSuccessionPlanQuotaView(FallbackSuccessionPlanQuotaView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedEndsAt;
    }
}
