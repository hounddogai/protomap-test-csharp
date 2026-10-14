namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default skill profile forecast.</summary>
public sealed record DefaultSkillProfileForecastView(string Code, string LabelText, string Approved, string StartsAtText, string Minimum, string TeamText, string StatusLabel)
{
    public static DefaultSkillProfileForecastView ToDefaultSkillProfileForecastView(DefaultSkillProfileForecast item)
    {
        return new DefaultSkillProfileForecastView(
            item.Code,
            Text.Truncate(item.Label, 120),
            $"{item.Approved}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultSkillProfileForecastView> ToDefaultSkillProfileForecastViews(List<DefaultSkillProfileForecast> items)
    {
        var views = new List<DefaultSkillProfileForecastView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultSkillProfileForecastView(item));
        }
        return views;
    }

    public static double SumDefaultSkillProfileForecastView(DefaultSkillProfileForecastView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMinimum;
    }
}
