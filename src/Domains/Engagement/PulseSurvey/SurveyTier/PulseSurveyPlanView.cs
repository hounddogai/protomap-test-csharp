namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey plan.</summary>
public sealed record PulseSurveyPlanView(string Code, string LabelText, string UpdatedAt, string RatingText, string Name, string Quantity, string Cost, string StatusLabel)
{
    public static PulseSurveyPlanView ToPulseSurveyPlanView(PulseSurveyPlan item)
    {
        return new PulseSurveyPlanView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyPlanView> ToPulseSurveyPlanViews(List<PulseSurveyPlan> items)
    {
        var views = new List<PulseSurveyPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyPlanView(item));
        }
        return views;
    }

    public static double SumPulseSurveyPlanView(PulseSurveyPlanView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRating + (double)parsedQuantity;
    }
}
