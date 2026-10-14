namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey template.</summary>
public sealed record PulseSurveyTemplateView(string CodeText, string LabelText, string Budget, string CategoryText, string SegmentText, string StatusLabel)
{
    public static PulseSurveyTemplateView ToPulseSurveyTemplateView(PulseSurveyTemplate item)
    {
        return new PulseSurveyTemplateView(
            item.Code,
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyTemplateView> ToPulseSurveyTemplateViews(List<PulseSurveyTemplate> items)
    {
        var views = new List<PulseSurveyTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyTemplateView(item));
        }
        return views;
    }

    public static double SumPulseSurveyTemplateView(PulseSurveyTemplateView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
