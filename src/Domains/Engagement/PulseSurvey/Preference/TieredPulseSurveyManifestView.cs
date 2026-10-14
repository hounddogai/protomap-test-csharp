namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered pulse survey manifest.</summary>
public sealed record TieredPulseSurveyManifestView(string Code, string Title, string JobCode, string CreatedAt, string UpdatedAt, string StatusLabel)
{
    public static TieredPulseSurveyManifestView ToTieredPulseSurveyManifestView(TieredPulseSurveyManifest item)
    {
        return new TieredPulseSurveyManifestView(
            item.Code,
            item.Title,
            Text.Truncate(item.JobCode, 40),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPulseSurveyManifestView> ToTieredPulseSurveyManifestViews(List<TieredPulseSurveyManifest> items)
    {
        var views = new List<TieredPulseSurveyManifestView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPulseSurveyManifestView(item));
        }
        return views;
    }

    public static double SumTieredPulseSurveyManifestView(TieredPulseSurveyManifestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedUpdatedAt;
    }
}
