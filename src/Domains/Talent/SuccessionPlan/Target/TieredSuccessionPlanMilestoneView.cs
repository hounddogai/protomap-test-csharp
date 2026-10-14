namespace Acme.Workforce.Talent.SuccessionPlan.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered succession plan milestone.</summary>
public sealed record TieredSuccessionPlanMilestoneView(string Code, string Name, string Grade, string OwnerIdText, string Rate, string CreatedAt, string CountryText, string AttemptsText, string LocaleText, string Retries, string Checksum, string StatusLabel)
{
    public static TieredSuccessionPlanMilestoneView ToTieredSuccessionPlanMilestoneView(TieredSuccessionPlanMilestone item)
    {
        return new TieredSuccessionPlanMilestoneView(
            item.Code,
            item.Name,
            item.Grade,
            item.OwnerId,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredSuccessionPlanMilestoneView> ToTieredSuccessionPlanMilestoneViews(List<TieredSuccessionPlanMilestone> items)
    {
        var views = new List<TieredSuccessionPlanMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToTieredSuccessionPlanMilestoneView(item));
        }
        return views;
    }

    public static double SumTieredSuccessionPlanMilestoneView(TieredSuccessionPlanMilestoneView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedCreatedAt + (double)parsedAttempts;
    }
}
