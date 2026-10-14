namespace Acme.Workforce.Talent.SkillProfile.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile group.</summary>
public sealed record SkillProfileGroupView(string CodeText, string Name, string Maximum, string ReviewerText, string Limit, string StatusLabel)
{
    public static SkillProfileGroupView ToSkillProfileGroupView(SkillProfileGroup item)
    {
        return new SkillProfileGroupView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileGroupView> ToSkillProfileGroupViews(List<SkillProfileGroup> items)
    {
        var views = new List<SkillProfileGroupView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileGroupView(item));
        }
        return views;
    }

    public static double SumSkillProfileGroupView(SkillProfileGroupView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedLimit;
    }
}
