namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global pulse survey record.</summary>
public sealed record GlobalPulseSurveyRecordView(string Code, string Name, string Weight, string Pinned, string EndsAt, string CeilingText, string Summary, string StatusLabel)
{
    public static GlobalPulseSurveyRecordView ToGlobalPulseSurveyRecordView(GlobalPulseSurveyRecord item)
    {
        return new GlobalPulseSurveyRecordView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPulseSurveyRecordView> ToGlobalPulseSurveyRecordViews(List<GlobalPulseSurveyRecord> items)
    {
        var views = new List<GlobalPulseSurveyRecordView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPulseSurveyRecordView(item));
        }
        return views;
    }

    public static double SumGlobalPulseSurveyRecordView(GlobalPulseSurveyRecordView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedEndsAt + parsedCeiling;
    }
}
