namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey rollup.</summary>
public sealed record PulseSurveyRollupView(string CodeText, string TitleText, string OwnerId, string AmountText, string Discount, string Notes, string GraceDays, string Label, string StatusLabel)
{
    public static PulseSurveyRollupView ToPulseSurveyRollupView(PulseSurveyRollup item)
    {
        return new PulseSurveyRollupView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.OwnerId, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyRollupView> ToPulseSurveyRollupViews(List<PulseSurveyRollup> items)
    {
        var views = new List<PulseSurveyRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyRollupView(item));
        }
        return views;
    }

    public static double SumPulseSurveyRollupView(PulseSurveyRollupView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedAmount + parsedDiscount + (double)parsedGraceDays;
    }
}
