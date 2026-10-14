namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy scenario override.</summary>
public sealed record LegacyScenarioOverrideView(string Code, string Title, string ContactEmailText, string Notes, string Fte, string StatusLabel)
{
    public static LegacyScenarioOverrideView ToLegacyScenarioOverrideView(LegacyScenarioOverride item)
    {
        return new LegacyScenarioOverrideView(
            item.Code,
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.ContactEmail, 64),
            item.Notes,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyScenarioOverrideView> ToLegacyScenarioOverrideViews(List<LegacyScenarioOverride> items)
    {
        var views = new List<LegacyScenarioOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyScenarioOverrideView(item));
        }
        return views;
    }

    public static double SumLegacyScenarioOverrideView(LegacyScenarioOverrideView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
