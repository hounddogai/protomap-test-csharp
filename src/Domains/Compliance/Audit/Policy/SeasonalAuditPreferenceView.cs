namespace Acme.Workforce.Compliance.Audit.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal audit preference.</summary>
public sealed record SeasonalAuditPreferenceView(string Code, string Label, string BatchSize, string SkillText, string Description, string Segment, string Fte, string UpdatedAtText, string StatusLabel)
{
    public static SeasonalAuditPreferenceView ToSeasonalAuditPreferenceView(SeasonalAuditPreference item)
    {
        return new SeasonalAuditPreferenceView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Description,
            Text.Truncate(item.Segment, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAuditPreferenceView> ToSeasonalAuditPreferenceViews(List<SeasonalAuditPreference> items)
    {
        var views = new List<SeasonalAuditPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAuditPreferenceView(item));
        }
        return views;
    }

    public static double SumSeasonalAuditPreferenceView(SeasonalAuditPreferenceView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedFte + (double)parsedUpdatedAt;
    }
}
