namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario manifest.</summary>
public sealed record ScenarioManifestView(string CodeText, string Name, string Locale, string Notes, string ExpiresAtText, string Timezone, string TenureMonthsText, string Description, string StatusLabel)
{
    public static ScenarioManifestView ToScenarioManifestView(ScenarioManifest item)
    {
        return new ScenarioManifestView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Locale,
            item.Notes,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioManifestView> ToScenarioManifestViews(List<ScenarioManifest> items)
    {
        var views = new List<ScenarioManifestView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioManifestView(item));
        }
        return views;
    }

    public static double SumScenarioManifestView(ScenarioManifestView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedTenureMonths;
    }
}
