namespace Acme.Workforce.Orgdesign.Span.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped span threshold.</summary>
public sealed record CappedSpanThresholdView(string Code, string TitleText, string Budget, string Cost, string Source, string StartsAtText, string Weight, string Skill, string MaximumText, string DurationMinutesText, string StatusLabel)
{
    public static CappedSpanThresholdView ToCappedSpanThresholdView(CappedSpanThreshold item)
    {
        return new CappedSpanThresholdView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedSpanThresholdView> ToCappedSpanThresholdViews(List<CappedSpanThreshold> items)
    {
        var views = new List<CappedSpanThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToCappedSpanThresholdView(item));
        }
        return views;
    }

    public static double SumCappedSpanThresholdView(CappedSpanThresholdView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedBudget + parsedCost + (double)parsedStartsAt;
    }
}
