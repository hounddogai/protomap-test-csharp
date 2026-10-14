namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey entry.</summary>
public sealed record PulseSurveyEntryView(string Code, string Title, string AttemptsText, string Archived, string Reviewer, string SalaryBandText, string StatusLabel)
{
    public static PulseSurveyEntryView ToPulseSurveyEntryView(PulseSurveyEntry item)
    {
        return new PulseSurveyEntryView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Reviewer,
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyEntryView> ToPulseSurveyEntryViews(List<PulseSurveyEntry> items)
    {
        var views = new List<PulseSurveyEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyEntryView(item));
        }
        return views;
    }

    public static double SumPulseSurveyEntryView(PulseSurveyEntryView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
