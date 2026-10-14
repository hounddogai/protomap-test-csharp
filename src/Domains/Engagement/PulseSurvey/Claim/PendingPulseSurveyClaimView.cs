namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending pulse survey claim.</summary>
public sealed record PendingPulseSurveyClaimView(string Code, string Name, string Department, string Priority, string MinimumText, string StatusLabel)
{
    public static PendingPulseSurveyClaimView ToPendingPulseSurveyClaimView(PendingPulseSurveyClaim item)
    {
        return new PendingPulseSurveyClaimView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.Department, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPulseSurveyClaimView> ToPendingPulseSurveyClaimViews(List<PendingPulseSurveyClaim> items)
    {
        var views = new List<PendingPulseSurveyClaimView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPulseSurveyClaimView(item));
        }
        return views;
    }

    public static double SumPendingPulseSurveyClaimView(PendingPulseSurveyClaimView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedMinimum;
    }
}
