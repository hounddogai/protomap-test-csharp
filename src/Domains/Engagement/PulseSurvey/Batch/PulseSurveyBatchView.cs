namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey batch.</summary>
public sealed record PulseSurveyBatchView(string Code, string TitleText, string RegionText, string JobCode, string Active, string CreatedAt, string Ratio, string StatusLabel)
{
    public static PulseSurveyBatchView ToPulseSurveyBatchView(PulseSurveyBatch item)
    {
        return new PulseSurveyBatchView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Region, 80),
            Text.Truncate(item.JobCode, 80),
            $"{item.Active}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyBatchView> ToPulseSurveyBatchViews(List<PulseSurveyBatch> items)
    {
        var views = new List<PulseSurveyBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyBatchView(item));
        }
        return views;
    }

    public static double SumPulseSurveyBatchView(PulseSurveyBatchView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedRatio;
    }
}
