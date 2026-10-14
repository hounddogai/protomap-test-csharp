namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using System.Globalization;

/// <summary>How screens and exports show a strict pulse survey plan.</summary>
public sealed record StrictPulseSurveyPlanView(string CodeText, string Label, string Skill, string CreatedAt, string Quantity, string StatusLabel)
{
    public static StrictPulseSurveyPlanView ToStrictPulseSurveyPlanView(StrictPulseSurveyPlan item)
    {
        return new StrictPulseSurveyPlanView(
            item.Code,
            item.Label,
            item.Skill,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPulseSurveyPlanView> ToStrictPulseSurveyPlanViews(List<StrictPulseSurveyPlan> items)
    {
        var views = new List<StrictPulseSurveyPlanView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPulseSurveyPlanView(item));
        }
        return views;
    }

    public static double SumStrictPulseSurveyPlanView(StrictPulseSurveyPlanView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedQuantity;
    }
}
