namespace Acme.Workforce.Compliance.Audit.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default audit tier.</summary>
public sealed record DefaultAuditTierView(string CodeText, string Label, string CreatedAt, string SkillText, string GraceDays, string Cost, string Rate, string Ceiling, string Unit, string Category, string StartsAt, string StatusLabel)
{
    public static DefaultAuditTierView ToDefaultAuditTierView(DefaultAuditTier item)
    {
        return new DefaultAuditTierView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            Text.Truncate(item.Category, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAuditTierView> ToDefaultAuditTierViews(List<DefaultAuditTier> items)
    {
        var views = new List<DefaultAuditTierView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAuditTierView(item));
        }
        return views;
    }

    public static double SumDefaultAuditTierView(DefaultAuditTierView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedGraceDays + parsedCost;
    }
}
