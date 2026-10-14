namespace Acme.Workforce.Talent.SkillProfile.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered skill profile proposal.</summary>
public sealed record TieredSkillProfileProposalView(string Code, string Label, string Region, string Grade, string WindowDaysText, string Version, string Notes, string StatusLabel)
{
    public static TieredSkillProfileProposalView ToTieredSkillProfileProposalView(TieredSkillProfileProposal item)
    {
        return new TieredSkillProfileProposalView(
            item.Code,
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Region, 64),
            Text.Truncate(item.Grade, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredSkillProfileProposalView> ToTieredSkillProfileProposalViews(List<TieredSkillProfileProposal> items)
    {
        var views = new List<TieredSkillProfileProposalView>();
        foreach (var item in items)
        {
            views.Add(ToTieredSkillProfileProposalView(item));
        }
        return views;
    }

    public static double SumTieredSkillProfileProposalView(TieredSkillProfileProposalView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedVersion;
    }
}
