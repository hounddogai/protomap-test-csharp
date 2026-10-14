namespace Acme.Workforce.Talent.SkillProfile.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile forecast.</summary>
public sealed record SkillProfileForecastView(string Code, string Name, string LeaveDays, string StartsAt, string StatusLabel)
{
    public static SkillProfileForecastView ToSkillProfileForecastView(SkillProfileForecast item)
    {
        return new SkillProfileForecastView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileForecastView> ToSkillProfileForecastViews(List<SkillProfileForecast> items)
    {
        var views = new List<SkillProfileForecastView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileForecastView(item));
        }
        return views;
    }

    public static double SumSkillProfileForecastView(SkillProfileForecastView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedStartsAt;
    }
}
