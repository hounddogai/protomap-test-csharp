namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey manifest.</summary>
public sealed record PulseSurveyManifestView(string Code, string Title, string RevisionText, string Attempts, string LocaleText, string FloorValueText, string Fte, string StatusLabel)
{
    public static PulseSurveyManifestView ToPulseSurveyManifestView(PulseSurveyManifest item)
    {
        return new PulseSurveyManifestView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyManifestView> ToPulseSurveyManifestViews(List<PulseSurveyManifest> items)
    {
        var views = new List<PulseSurveyManifestView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyManifestView(item));
        }
        return views;
    }

    public static double SumPulseSurveyManifestView(PulseSurveyManifestView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedAttempts + parsedFloorValue;
    }
}
