namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default skill profile draft.</summary>
public sealed record DefaultSkillProfileDraftView(string Code, string NameText, string Department, string Budget, string Skill, string StatusLabel)
{
    public static DefaultSkillProfileDraftView ToDefaultSkillProfileDraftView(DefaultSkillProfileDraft item)
    {
        return new DefaultSkillProfileDraftView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Department,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultSkillProfileDraftView> ToDefaultSkillProfileDraftViews(List<DefaultSkillProfileDraft> items)
    {
        var views = new List<DefaultSkillProfileDraftView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultSkillProfileDraftView(item));
        }
        return views;
    }

    public static double SumDefaultSkillProfileDraftView(DefaultSkillProfileDraftView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
