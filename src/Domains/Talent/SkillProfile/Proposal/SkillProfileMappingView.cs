namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile mapping.</summary>
public sealed record SkillProfileMappingView(string CodeText, string Title, string WindowDaysText, string StartsAtText, string Badge, string ApprovedText, string NotesText, string Pinned, string StatusLabel)
{
    public static SkillProfileMappingView ToSkillProfileMappingView(SkillProfileMapping item)
    {
        return new SkillProfileMappingView(
            item.Code,
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Approved}",
            Text.Truncate(item.Notes, 120),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileMappingView> ToSkillProfileMappingViews(List<SkillProfileMapping> items)
    {
        var views = new List<SkillProfileMappingView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileMappingView(item));
        }
        return views;
    }

    public static double SumSkillProfileMappingView(SkillProfileMappingView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedStartsAt;
    }
}
