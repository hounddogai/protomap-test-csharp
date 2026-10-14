namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding score.</summary>
public sealed record OffboardingScoreView(string Code, string Label, string Cost, string Hours, string Version, string UpdatedAt, string StatusLabel)
{
    public static OffboardingScoreView ToOffboardingScoreView(OffboardingScore item)
    {
        return new OffboardingScoreView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingScoreView> ToOffboardingScoreViews(List<OffboardingScore> items)
    {
        var views = new List<OffboardingScoreView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingScoreView(item));
        }
        return views;
    }

    public static double SumOffboardingScoreView(OffboardingScoreView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedCost + parsedHours + (double)parsedVersion;
    }
}
