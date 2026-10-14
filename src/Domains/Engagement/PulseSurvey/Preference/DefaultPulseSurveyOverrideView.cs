namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using System.Globalization;

/// <summary>How screens and exports show a default pulse survey override.</summary>
public sealed record DefaultPulseSurveyOverrideView(string CodeText, string NameText, string LocaleText, string ExternalRefText, string SalaryBand, string CreatedAt, string StartsAt, string EndsAt, string Capacity, string Category, string StatusLabel)
{
    public static DefaultPulseSurveyOverrideView ToDefaultPulseSurveyOverrideView(DefaultPulseSurveyOverride item)
    {
        return new DefaultPulseSurveyOverrideView(
            item.Code,
            item.Name,
            item.Locale,
            item.ExternalRef,
            item.SalaryBand,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPulseSurveyOverrideView> ToDefaultPulseSurveyOverrideViews(List<DefaultPulseSurveyOverride> items)
    {
        var views = new List<DefaultPulseSurveyOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPulseSurveyOverrideView(item));
        }
        return views;
    }

    public static double SumDefaultPulseSurveyOverrideView(DefaultPulseSurveyOverrideView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedStartsAt + (double)parsedEndsAt;
    }
}
