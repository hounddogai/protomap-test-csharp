namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global offboarding score.</summary>
public sealed record GlobalOffboardingScoreView(string Code, string TitleText, string FloorValue, string Ratio, string Department, string Source, string SegmentText, string StatusLabel)
{
    public static GlobalOffboardingScoreView ToGlobalOffboardingScoreView(GlobalOffboardingScore item)
    {
        return new GlobalOffboardingScoreView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 120),
            Text.Truncate(item.Source, 120),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalOffboardingScoreView> ToGlobalOffboardingScoreViews(List<GlobalOffboardingScore> items)
    {
        var views = new List<GlobalOffboardingScoreView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalOffboardingScoreView(item));
        }
        return views;
    }

    public static double SumGlobalOffboardingScoreView(GlobalOffboardingScoreView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedRatio;
    }
}
