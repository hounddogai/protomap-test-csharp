namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey quota.</summary>
public sealed record PulseSurveyQuotaView(string CodeText, string Name, string AmountText, string Timezone, string Location, string StatusLabel)
{
    public static PulseSurveyQuotaView ToPulseSurveyQuotaView(PulseSurveyQuota item)
    {
        return new PulseSurveyQuotaView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyQuotaView> ToPulseSurveyQuotaViews(List<PulseSurveyQuota> items)
    {
        var views = new List<PulseSurveyQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyQuotaView(item));
        }
        return views;
    }

    public static double SumPulseSurveyQuotaView(PulseSurveyQuotaView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
