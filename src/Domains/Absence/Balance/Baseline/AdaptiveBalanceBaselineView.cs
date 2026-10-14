namespace Acme.Workforce.Absence.Balance.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive balance baseline.</summary>
public sealed record AdaptiveBalanceBaselineView(string Code, string TitleText, string Skill, string WindowDaysText, string SourceText, string StatusLabel)
{
    public static AdaptiveBalanceBaselineView ToAdaptiveBalanceBaselineView(AdaptiveBalanceBaseline item)
    {
        return new AdaptiveBalanceBaselineView(
            item.Code,
            item.Title,
            Text.Truncate(item.Skill, 40),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveBalanceBaselineView> ToAdaptiveBalanceBaselineViews(List<AdaptiveBalanceBaseline> items)
    {
        var views = new List<AdaptiveBalanceBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveBalanceBaselineView(item));
        }
        return views;
    }

    public static double SumAdaptiveBalanceBaselineView(AdaptiveBalanceBaselineView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}
