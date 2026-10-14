namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey draft.</summary>
public sealed record PulseSurveyDraftView(string CodeText, string NameText, string ActiveText, string ManagerId, string SalaryBandText, string Location, string AmountText, string StatusLabel)
{
    public static PulseSurveyDraftView ToPulseSurveyDraftView(PulseSurveyDraft item)
    {
        return new PulseSurveyDraftView(
            item.Code,
            Text.Truncate(item.Name, 40),
            $"{item.Active}",
            Text.Truncate(item.ManagerId, 40),
            item.SalaryBand,
            item.Location,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyDraftView> ToPulseSurveyDraftViews(List<PulseSurveyDraft> items)
    {
        var views = new List<PulseSurveyDraftView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyDraftView(item));
        }
        return views;
    }

    public static double SumPulseSurveyDraftView(PulseSurveyDraftView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
