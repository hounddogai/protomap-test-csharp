namespace Acme.Workforce.Orgdesign.Scenario.Template;

using System.Globalization;

/// <summary>How screens and exports show a scenario template.</summary>
public sealed record ScenarioTemplateView(string CodeText, string LabelText, string JobCodeText, string ActiveText, string Phone, string RatingText, string Limit, string Ceiling, string StatusLabel)
{
    public static ScenarioTemplateView ToScenarioTemplateView(ScenarioTemplate item)
    {
        return new ScenarioTemplateView(
            item.Code,
            item.Label,
            item.JobCode,
            $"{item.Active}",
            item.Phone,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioTemplateView> ToScenarioTemplateViews(List<ScenarioTemplate> items)
    {
        var views = new List<ScenarioTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioTemplateView(item));
        }
        return views;
    }

    public static double SumScenarioTemplateView(ScenarioTemplateView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedLimit + parsedCeiling;
    }
}
