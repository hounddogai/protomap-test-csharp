namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using System.Globalization;

/// <summary>How screens and exports show a pulse survey allocation.</summary>
public sealed record PulseSurveyAllocationView(string Code, string Label, string Ratio, string ExternalRef, string Approved, string DurationMinutes, string StatusLabel)
{
    public static PulseSurveyAllocationView ToPulseSurveyAllocationView(PulseSurveyAllocation item)
    {
        return new PulseSurveyAllocationView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Approved}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyAllocationView> ToPulseSurveyAllocationViews(List<PulseSurveyAllocation> items)
    {
        var views = new List<PulseSurveyAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyAllocationView(item));
        }
        return views;
    }

    public static double SumPulseSurveyAllocationView(PulseSurveyAllocationView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedDurationMinutes;
    }
}
