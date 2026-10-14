namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary reorg preference.</summary>
public sealed record PrimaryReorgPreferenceView(string Code, string Name, string Region, string Team, string Ratio, string Title, string ExternalRef, string Reviewer, string Phone, string Category, string CostText, string StatusLabel)
{
    public static PrimaryReorgPreferenceView ToPrimaryReorgPreferenceView(PrimaryReorgPreference item)
    {
        return new PrimaryReorgPreferenceView(
            item.Code,
            item.Name,
            Text.Truncate(item.Region, 120),
            item.Team,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.ExternalRef,
            item.Reviewer,
            item.Phone,
            Text.Truncate(item.Category, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryReorgPreferenceView> ToPrimaryReorgPreferenceViews(List<PrimaryReorgPreference> items)
    {
        var views = new List<PrimaryReorgPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryReorgPreferenceView(item));
        }
        return views;
    }

    public static double SumPrimaryReorgPreferenceView(PrimaryReorgPreferenceView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCost;
    }
}
