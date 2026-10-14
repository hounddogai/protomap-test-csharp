namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pulse survey preference.</summary>
public sealed record StrictPulseSurveyPreferenceView(string Code, string Name, string WindowDaysText, string Grade, string Active, string Country, string Category, string StatusLabel)
{
    public static StrictPulseSurveyPreferenceView ToStrictPulseSurveyPreferenceView(StrictPulseSurveyPreference item)
    {
        return new StrictPulseSurveyPreferenceView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Active}",
            Text.Truncate(item.Country, 64),
            Text.Truncate(item.Category, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPulseSurveyPreferenceView> ToStrictPulseSurveyPreferenceViews(List<StrictPulseSurveyPreference> items)
    {
        var views = new List<StrictPulseSurveyPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPulseSurveyPreferenceView(item));
        }
        return views;
    }

    public static double SumStrictPulseSurveyPreferenceView(StrictPulseSurveyPreferenceView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}
