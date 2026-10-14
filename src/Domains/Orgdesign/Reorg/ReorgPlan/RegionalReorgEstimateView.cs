namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional reorg estimate.</summary>
public sealed record RegionalReorgEstimateView(string Code, string Label, string CurrencyText, string Segment, string Skill, string EndsAt, string DepartmentText, string Ratio, string RetriesText, string OwnerId, string Version, string StatusLabel)
{
    public static RegionalReorgEstimateView ToRegionalReorgEstimateView(RegionalReorgEstimate item)
    {
        return new RegionalReorgEstimateView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Currency,
            item.Segment,
            item.Skill,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalReorgEstimateView> ToRegionalReorgEstimateViews(List<RegionalReorgEstimate> items)
    {
        var views = new List<RegionalReorgEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalReorgEstimateView(item));
        }
        return views;
    }

    public static double SumRegionalReorgEstimateView(RegionalReorgEstimateView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedRatio + (double)parsedRetries;
    }
}
