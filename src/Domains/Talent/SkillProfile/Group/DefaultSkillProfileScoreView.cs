namespace Acme.Workforce.Talent.SkillProfile.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default skill profile score.</summary>
public sealed record DefaultSkillProfileScoreView(string Code, string LabelText, string Skill, string Discount, string ExpiresAt, string TitleText, string StatusLabel)
{
    public static DefaultSkillProfileScoreView ToDefaultSkillProfileScoreView(DefaultSkillProfileScore item)
    {
        return new DefaultSkillProfileScoreView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Skill,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultSkillProfileScoreView> ToDefaultSkillProfileScoreViews(List<DefaultSkillProfileScore> items)
    {
        var views = new List<DefaultSkillProfileScoreView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultSkillProfileScoreView(item));
        }
        return views;
    }

    public static double SumDefaultSkillProfileScoreView(DefaultSkillProfileScoreView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedExpiresAt;
    }
}
