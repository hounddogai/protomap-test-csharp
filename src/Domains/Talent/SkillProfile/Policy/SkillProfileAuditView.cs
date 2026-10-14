namespace Acme.Workforce.Talent.SkillProfile.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile audit.</summary>
public sealed record SkillProfileAuditView(string Code, string TitleText, string UpdatedAt, string Margin, string Checksum, string Archived, string TimezoneText, string CostCenter, string ExternalRef, string OwnerId, string StatusLabel)
{
    public static SkillProfileAuditView ToSkillProfileAuditView(SkillProfileAudit item)
    {
        return new SkillProfileAuditView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Archived}",
            item.Timezone,
            item.CostCenter,
            item.ExternalRef,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileAuditView> ToSkillProfileAuditViews(List<SkillProfileAudit> items)
    {
        var views = new List<SkillProfileAuditView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileAuditView(item));
        }
        return views;
    }

    public static double SumSkillProfileAuditView(SkillProfileAuditView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMargin;
    }
}
