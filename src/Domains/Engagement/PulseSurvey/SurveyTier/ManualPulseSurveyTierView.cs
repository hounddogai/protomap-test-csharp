namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual pulse survey tier.</summary>
public sealed record ManualPulseSurveyTierView(string Code, string Label, string TitleText, string Region, string ExternalRef, string Source, string Location, string Unit, string Amount, string StatusLabel)
{
    public static ManualPulseSurveyTierView ToManualPulseSurveyTierView(ManualPulseSurveyTier item)
    {
        return new ManualPulseSurveyTierView(
            item.Code,
            item.Label,
            item.Title,
            item.Region,
            Text.Truncate(item.ExternalRef, 80),
            Text.Truncate(item.Source, 80),
            item.Location,
            item.Unit,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPulseSurveyTierView> ToManualPulseSurveyTierViews(List<ManualPulseSurveyTier> items)
    {
        var views = new List<ManualPulseSurveyTierView>();
        foreach (var item in items)
        {
            views.Add(ToManualPulseSurveyTierView(item));
        }
        return views;
    }

    public static double SumManualPulseSurveyTierView(ManualPulseSurveyTierView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
