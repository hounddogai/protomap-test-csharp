namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a pulse survey estimate.</summary>
public sealed record PulseSurveyEstimateView(string Code, string Name, string BatchSize, string OwnerIdText, string Country, string Version, string Active, string StatusLabel)
{
    public static PulseSurveyEstimateView ToPulseSurveyEstimateView(PulseSurveyEstimate item)
    {
        return new PulseSurveyEstimateView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Country,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyEstimateView> ToPulseSurveyEstimateViews(List<PulseSurveyEstimate> items)
    {
        var views = new List<PulseSurveyEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyEstimateView(item));
        }
        return views;
    }

    public static double SumPulseSurveyEstimateView(PulseSurveyEstimateView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedVersion;
    }
}
