namespace Acme.Workforce.Talent.SkillProfile.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile exception.</summary>
public sealed record SkillProfileExceptionView(string Code, string Title, string Description, string StartsAtText, string ThresholdText, string Attempts, string Weight, string StatusLabel)
{
    public static SkillProfileExceptionView ToSkillProfileExceptionView(SkillProfileException item)
    {
        return new SkillProfileExceptionView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Description,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileExceptionView> ToSkillProfileExceptionViews(List<SkillProfileException> items)
    {
        var views = new List<SkillProfileExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileExceptionView(item));
        }
        return views;
    }

    public static double SumSkillProfileExceptionView(SkillProfileExceptionView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedThreshold + (double)parsedAttempts;
    }
}
