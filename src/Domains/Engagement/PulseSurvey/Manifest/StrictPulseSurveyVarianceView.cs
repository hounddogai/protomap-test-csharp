namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using System.Globalization;

/// <summary>How screens and exports show a strict pulse survey variance.</summary>
public sealed record StrictPulseSurveyVarianceView(string CodeText, string Title, string Approved, string StartsAtText, string ExpiresAt, string AmountText, string Archived, string Capacity, string StatusLabel)
{
    public static StrictPulseSurveyVarianceView ToStrictPulseSurveyVarianceView(StrictPulseSurveyVariance item)
    {
        return new StrictPulseSurveyVarianceView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPulseSurveyVarianceView> ToStrictPulseSurveyVarianceViews(List<StrictPulseSurveyVariance> items)
    {
        var views = new List<StrictPulseSurveyVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPulseSurveyVarianceView(item));
        }
        return views;
    }

    public static double SumStrictPulseSurveyVarianceView(StrictPulseSurveyVarianceView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedExpiresAt + parsedAmount;
    }
}
