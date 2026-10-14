namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile summary.</summary>
public sealed record SkillProfileSummaryView(string CodeText, string Name, string Skill, string LabelText, string Checksum, string Notes, string ExternalRef, string RateText, string Team, string Version, string StatusLabel)
{
    public static SkillProfileSummaryView ToSkillProfileSummaryView(SkillProfileSummary item)
    {
        return new SkillProfileSummaryView(
            item.Code,
            item.Name,
            item.Skill,
            item.Label,
            item.Checksum,
            item.Notes,
            item.ExternalRef,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileSummaryView> ToSkillProfileSummaryViews(List<SkillProfileSummary> items)
    {
        var views = new List<SkillProfileSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileSummaryView(item));
        }
        return views;
    }

    public static double SumSkillProfileSummaryView(SkillProfileSummaryView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedVersion;
    }
}
