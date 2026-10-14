namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using System.Globalization;

/// <summary>How screens and exports show a pulse survey profile.</summary>
public sealed record PulseSurveyProfileView(string Code, string NameText, string Rate, string Ratio, string Segment, string EmployeeId, string Unit, string StatusLabel)
{
    public static PulseSurveyProfileView ToPulseSurveyProfileView(PulseSurveyProfile item)
    {
        return new PulseSurveyProfileView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.EmployeeId,
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyProfileView> ToPulseSurveyProfileViews(List<PulseSurveyProfile> items)
    {
        var views = new List<PulseSurveyProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyProfileView(item));
        }
        return views;
    }

    public static double SumPulseSurveyProfileView(PulseSurveyProfileView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRate + parsedRatio;
    }
}
