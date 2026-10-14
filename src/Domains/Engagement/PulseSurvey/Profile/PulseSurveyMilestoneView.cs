namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey milestone.</summary>
public sealed record PulseSurveyMilestoneView(string Code, string Label, string RatingText, string Fte, string Ceiling, string DurationMinutesText, string BadgeText, string CostCenter, string WindowDaysText, string Budget, string StatusLabel)
{
    public static PulseSurveyMilestoneView ToPulseSurveyMilestoneView(PulseSurveyMilestone item)
    {
        return new PulseSurveyMilestoneView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.CostCenter, 40),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyMilestoneView> ToPulseSurveyMilestoneViews(List<PulseSurveyMilestone> items)
    {
        var views = new List<PulseSurveyMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyMilestoneView(item));
        }
        return views;
    }

    public static double SumPulseSurveyMilestoneView(PulseSurveyMilestoneView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedRating + parsedFte + parsedCeiling;
    }
}
