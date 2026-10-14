namespace Acme.Workforce.Talent.SkillProfile.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile template.</summary>
public sealed record SkillProfileTemplateView(string Code, string LabelText, string Cost, string Score, string Reviewer, string StatusLabel)
{
    public static SkillProfileTemplateView ToSkillProfileTemplateView(SkillProfileTemplate item)
    {
        return new SkillProfileTemplateView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileTemplateView> ToSkillProfileTemplateViews(List<SkillProfileTemplate> items)
    {
        var views = new List<SkillProfileTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileTemplateView(item));
        }
        return views;
    }

    public static double SumSkillProfileTemplateView(SkillProfileTemplateView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedCost + parsedScore;
    }
}
