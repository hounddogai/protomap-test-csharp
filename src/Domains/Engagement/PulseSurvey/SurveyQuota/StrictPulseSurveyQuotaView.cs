namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using System.Globalization;

/// <summary>How screens and exports show a strict pulse survey quota.</summary>
public sealed record StrictPulseSurveyQuotaView(string Code, string Name, string ContactEmail, string Capacity, string DepartmentText, string WindowDays, string Weight, string Title, string Description, string StatusLabel)
{
    public static StrictPulseSurveyQuotaView ToStrictPulseSurveyQuotaView(StrictPulseSurveyQuota item)
    {
        return new StrictPulseSurveyQuotaView(
            item.Code,
            item.Name,
            item.ContactEmail,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPulseSurveyQuotaView> ToStrictPulseSurveyQuotaViews(List<StrictPulseSurveyQuota> items)
    {
        var views = new List<StrictPulseSurveyQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPulseSurveyQuotaView(item));
        }
        return views;
    }

    public static double SumStrictPulseSurveyQuotaView(StrictPulseSurveyQuotaView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedWindowDays + parsedWeight;
    }
}
