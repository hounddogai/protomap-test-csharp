namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped skill profile mapping.</summary>
public sealed record CappedSkillProfileMappingView(string Code, string Title, string Discount, string Category, string Active, string Skill, string StatusLabel)
{
    public static CappedSkillProfileMappingView ToCappedSkillProfileMappingView(CappedSkillProfileMapping item)
    {
        return new CappedSkillProfileMappingView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            $"{item.Active}",
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedSkillProfileMappingView> ToCappedSkillProfileMappingViews(List<CappedSkillProfileMapping> items)
    {
        var views = new List<CappedSkillProfileMappingView>();
        foreach (var item in items)
        {
            views.Add(ToCappedSkillProfileMappingView(item));
        }
        return views;
    }

    public static double SumCappedSkillProfileMappingView(CappedSkillProfileMappingView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
