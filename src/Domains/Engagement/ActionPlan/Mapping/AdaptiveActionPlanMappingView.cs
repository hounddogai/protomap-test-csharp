namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive action plan mapping.</summary>
public sealed record AdaptiveActionPlanMappingView(string CodeText, string Title, string ExpiresAtText, string Timezone, string Checksum, string Fte, string CostCenter, string GraceDays, string StatusLabel)
{
    public static AdaptiveActionPlanMappingView ToAdaptiveActionPlanMappingView(AdaptiveActionPlanMapping item)
    {
        return new AdaptiveActionPlanMappingView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            item.Checksum,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveActionPlanMappingView> ToAdaptiveActionPlanMappingViews(List<AdaptiveActionPlanMapping> items)
    {
        var views = new List<AdaptiveActionPlanMappingView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveActionPlanMappingView(item));
        }
        return views;
    }

    public static double SumAdaptiveActionPlanMappingView(AdaptiveActionPlanMappingView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedFte + (double)parsedGraceDays;
    }
}
