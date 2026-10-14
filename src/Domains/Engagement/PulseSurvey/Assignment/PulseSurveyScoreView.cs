namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey score.</summary>
public sealed record PulseSurveyScoreView(string Code, string NameText, string CreatedAt, string Archived, string Unit, string Timezone, string GraceDays, string TeamText, string RegionText, string LabelText, string StatusLabel)
{
    public static PulseSurveyScoreView ToPulseSurveyScoreView(PulseSurveyScore item)
    {
        return new PulseSurveyScoreView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Unit,
            item.Timezone,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 40),
            item.Region,
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyScoreView> ToPulseSurveyScoreViews(List<PulseSurveyScore> items)
    {
        var views = new List<PulseSurveyScoreView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyScoreView(item));
        }
        return views;
    }

    public static double SumPulseSurveyScoreView(PulseSurveyScoreView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedGraceDays;
    }
}
