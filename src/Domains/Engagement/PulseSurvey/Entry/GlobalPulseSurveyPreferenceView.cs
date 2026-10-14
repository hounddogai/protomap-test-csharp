namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using System.Globalization;

/// <summary>How screens and exports show a global pulse survey preference.</summary>
public sealed record GlobalPulseSurveyPreferenceView(string Code, string Title, string Source, string Grade, string Rate, string OwnerId, string Pinned, string Budget, string StatusLabel)
{
    public static GlobalPulseSurveyPreferenceView ToGlobalPulseSurveyPreferenceView(GlobalPulseSurveyPreference item)
    {
        return new GlobalPulseSurveyPreferenceView(
            item.Code,
            item.Title,
            item.Source,
            item.Grade,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Pinned}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPulseSurveyPreferenceView> ToGlobalPulseSurveyPreferenceViews(List<GlobalPulseSurveyPreference> items)
    {
        var views = new List<GlobalPulseSurveyPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPulseSurveyPreferenceView(item));
        }
        return views;
    }

    public static double SumGlobalPulseSurveyPreferenceView(GlobalPulseSurveyPreferenceView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedRate + parsedBudget;
    }
}
