namespace Acme.Workforce.Talent.SkillProfile.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile notice.</summary>
public sealed record SkillProfileNoticeView(string Code, string TitleText, string Team, string Currency, string SalaryBand, string ScoreText, string StatusLabel)
{
    public static SkillProfileNoticeView ToSkillProfileNoticeView(SkillProfileNotice item)
    {
        return new SkillProfileNoticeView(
            item.Code,
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.Team, 120),
            item.Currency,
            Text.Truncate(item.SalaryBand, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileNoticeView> ToSkillProfileNoticeViews(List<SkillProfileNotice> items)
    {
        var views = new List<SkillProfileNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileNoticeView(item));
        }
        return views;
    }

    public static double SumSkillProfileNoticeView(SkillProfileNoticeView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
