namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped pulse survey profile.</summary>
public sealed record CappedPulseSurveyProfileView(string Code, string Title, string Checksum, string Weight, string Revision, string Approved, string FloorValueText, string Archived, string Currency, string Maximum, string Limit, string StatusLabel)
{
    public static CappedPulseSurveyProfileView ToCappedPulseSurveyProfileView(CappedPulseSurveyProfile item)
    {
        return new CappedPulseSurveyProfileView(
            item.Code,
            item.Title,
            item.Checksum,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Currency, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPulseSurveyProfileView> ToCappedPulseSurveyProfileViews(List<CappedPulseSurveyProfile> items)
    {
        var views = new List<CappedPulseSurveyProfileView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPulseSurveyProfileView(item));
        }
        return views;
    }

    public static double SumCappedPulseSurveyProfileView(CappedPulseSurveyProfileView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedRevision + parsedFloorValue;
    }
}
