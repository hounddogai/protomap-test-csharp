namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a strict pulse survey override.</summary>
public sealed record StrictPulseSurveyOverrideView(string Code, string Label, string Discount, string EndsAt, string Phone, string Checksum, string UpdatedAtText, string Approved, string Department, string StatusLabel)
{
    public static StrictPulseSurveyOverrideView ToStrictPulseSurveyOverrideView(StrictPulseSurveyOverride item)
    {
        return new StrictPulseSurveyOverrideView(
            item.Code,
            item.Label,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Checksum,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPulseSurveyOverrideView> ToStrictPulseSurveyOverrideViews(List<StrictPulseSurveyOverride> items)
    {
        var views = new List<StrictPulseSurveyOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPulseSurveyOverrideView(item));
        }
        return views;
    }

    public static double SumStrictPulseSurveyOverrideView(StrictPulseSurveyOverrideView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedEndsAt + (double)parsedUpdatedAt;
    }
}
