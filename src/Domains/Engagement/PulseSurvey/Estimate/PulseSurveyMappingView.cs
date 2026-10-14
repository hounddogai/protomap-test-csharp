namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey mapping.</summary>
public sealed record PulseSurveyMappingView(string Code, string Label, string WindowDays, string Maximum, string EndsAtText, string Sequence, string CostText, string Retries, string Priority, string StatusLabel)
{
    public static PulseSurveyMappingView ToPulseSurveyMappingView(PulseSurveyMapping item)
    {
        return new PulseSurveyMappingView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyMappingView> ToPulseSurveyMappingViews(List<PulseSurveyMapping> items)
    {
        var views = new List<PulseSurveyMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyMappingView(item));
        }
        return views;
    }

    public static double SumPulseSurveyMappingView(PulseSurveyMappingView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMaximum + (double)parsedEndsAt;
    }
}
