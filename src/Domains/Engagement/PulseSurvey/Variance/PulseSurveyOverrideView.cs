namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey override.</summary>
public sealed record PulseSurveyOverrideView(string Code, string NameText, string Unit, string Label, string Ratio, string Category, string StatusLabel)
{
    public static PulseSurveyOverrideView ToPulseSurveyOverrideView(PulseSurveyOverride item)
    {
        return new PulseSurveyOverrideView(
            item.Code,
            item.Name,
            item.Unit,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyOverrideView> ToPulseSurveyOverrideViews(List<PulseSurveyOverride> items)
    {
        var views = new List<PulseSurveyOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyOverrideView(item));
        }
        return views;
    }

    public static double SumPulseSurveyOverrideView(PulseSurveyOverrideView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
