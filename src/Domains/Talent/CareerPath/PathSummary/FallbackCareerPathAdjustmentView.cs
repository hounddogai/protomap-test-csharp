namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback career path adjustment.</summary>
public sealed record FallbackCareerPathAdjustmentView(string Code, string Label, string ManagerIdText, string FloorValueText, string EndsAt, string Minimum, string Badge, string StatusLabel)
{
    public static FallbackCareerPathAdjustmentView ToFallbackCareerPathAdjustmentView(FallbackCareerPathAdjustment item)
    {
        return new FallbackCareerPathAdjustmentView(
            Text.Truncate(item.Code, 120),
            item.Label,
            Text.Truncate(item.ManagerId, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackCareerPathAdjustmentView> ToFallbackCareerPathAdjustmentViews(List<FallbackCareerPathAdjustment> items)
    {
        var views = new List<FallbackCareerPathAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackCareerPathAdjustmentView(item));
        }
        return views;
    }

    public static double SumFallbackCareerPathAdjustmentView(FallbackCareerPathAdjustmentView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedEndsAt + parsedMinimum;
    }
}
