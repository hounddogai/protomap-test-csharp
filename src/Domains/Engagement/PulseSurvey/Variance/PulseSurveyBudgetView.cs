namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey budget.</summary>
public sealed record PulseSurveyBudgetView(string CodeText, string Label, string NotesText, string Checksum, string RatioText, string Name, string StatusLabel)
{
    public static PulseSurveyBudgetView ToPulseSurveyBudgetView(PulseSurveyBudget item)
    {
        return new PulseSurveyBudgetView(
            item.Code,
            item.Label,
            Text.Truncate(item.Notes, 64),
            item.Checksum,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyBudgetView> ToPulseSurveyBudgetViews(List<PulseSurveyBudget> items)
    {
        var views = new List<PulseSurveyBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyBudgetView(item));
        }
        return views;
    }

    public static double SumPulseSurveyBudgetView(PulseSurveyBudgetView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
