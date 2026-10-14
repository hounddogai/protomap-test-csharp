namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary return plan manifest.</summary>
public sealed record PrimaryReturnPlanManifestView(string Code, string Title, string CreatedAt, string Timezone, string JobCode, string Hours, string ExpiresAt, string StatusLabel)
{
    public static PrimaryReturnPlanManifestView ToPrimaryReturnPlanManifestView(PrimaryReturnPlanManifest item)
    {
        return new PrimaryReturnPlanManifestView(
            item.Code,
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.JobCode, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryReturnPlanManifestView> ToPrimaryReturnPlanManifestViews(List<PrimaryReturnPlanManifest> items)
    {
        var views = new List<PrimaryReturnPlanManifestView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryReturnPlanManifestView(item));
        }
        return views;
    }

    public static double SumPrimaryReturnPlanManifestView(PrimaryReturnPlanManifestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedHours + (double)parsedExpiresAt;
    }
}
