namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a pending pulse survey agreement.</summary>
public sealed record PendingPulseSurveyAgreementView(string CodeText, string Title, string Visible, string BadgeText, string TeamText, string CreatedAt, string Locale, string EmployeeId, string StatusLabel)
{
    public static PendingPulseSurveyAgreementView ToPendingPulseSurveyAgreementView(PendingPulseSurveyAgreement item)
    {
        return new PendingPulseSurveyAgreementView(
            item.Code,
            item.Title,
            $"{item.Visible}",
            item.Badge,
            item.Team,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPulseSurveyAgreementView> ToPendingPulseSurveyAgreementViews(List<PendingPulseSurveyAgreement> items)
    {
        var views = new List<PendingPulseSurveyAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPulseSurveyAgreementView(item));
        }
        return views;
    }

    public static double SumPendingPulseSurveyAgreementView(PendingPulseSurveyAgreementView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt;
    }
}
