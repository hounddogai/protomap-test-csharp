namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey record.</summary>
public sealed record PulseSurveyRecordView(string CodeText, string NameText, string DescriptionText, string Locale, string LeaveDaysText, string TenureMonths, string StatusLabel)
{
    public static PulseSurveyRecordView ToPulseSurveyRecordView(PulseSurveyRecord item)
    {
        return new PulseSurveyRecordView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Description,
            item.Locale,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyRecordView> ToPulseSurveyRecordViews(List<PulseSurveyRecord> items)
    {
        var views = new List<PulseSurveyRecordView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyRecordView(item));
        }
        return views;
    }

    public static double SumPulseSurveyRecordView(PulseSurveyRecordView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedTenureMonths;
    }
}
