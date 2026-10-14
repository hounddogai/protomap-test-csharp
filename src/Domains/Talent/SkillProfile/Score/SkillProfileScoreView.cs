namespace Acme.Workforce.Talent.SkillProfile.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile score.</summary>
public sealed record SkillProfileScoreView(string CodeText, string NameText, string Weight, string Description, string Title, string Score, string StatusLabel)
{
    public static SkillProfileScoreView ToSkillProfileScoreView(SkillProfileScore item)
    {
        return new SkillProfileScoreView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileScoreView> ToSkillProfileScoreViews(List<SkillProfileScore> items)
    {
        var views = new List<SkillProfileScoreView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileScoreView(item));
        }
        return views;
    }

    public static double SumSkillProfileScoreView(SkillProfileScoreView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedWeight + parsedScore;
    }
}
