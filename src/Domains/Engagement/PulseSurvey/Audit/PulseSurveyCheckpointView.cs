namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using System.Globalization;

/// <summary>How screens and exports show a pulse survey checkpoint.</summary>
public sealed record PulseSurveyCheckpointView(string Code, string LabelText, string Grade, string JobCode, string TitleText, string Location, string EndsAtText, string Budget, string FloorValue, string StatusLabel)
{
    public static PulseSurveyCheckpointView ToPulseSurveyCheckpointView(PulseSurveyCheckpoint item)
    {
        return new PulseSurveyCheckpointView(
            item.Code,
            item.Label,
            item.Grade,
            item.JobCode,
            item.Title,
            item.Location,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyCheckpointView> ToPulseSurveyCheckpointViews(List<PulseSurveyCheckpoint> items)
    {
        var views = new List<PulseSurveyCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyCheckpointView(item));
        }
        return views;
    }

    public static double SumPulseSurveyCheckpointView(PulseSurveyCheckpointView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedBudget + parsedFloorValue;
    }
}
