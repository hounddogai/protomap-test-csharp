namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey slot.</summary>
public sealed record PulseSurveySlotView(string Code, string Name, string Minimum, string SummaryText, string Attempts, string SalaryBand, string Fte, string Phone, string FloorValue, string StatusLabel)
{
    public static PulseSurveySlotView ToPulseSurveySlotView(PulseSurveySlot item)
    {
        return new PulseSurveySlotView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveySlotView> ToPulseSurveySlotViews(List<PulseSurveySlot> items)
    {
        var views = new List<PulseSurveySlotView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveySlotView(item));
        }
        return views;
    }

    public static double SumPulseSurveySlotView(PulseSurveySlotView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedAttempts + parsedFte;
    }
}
