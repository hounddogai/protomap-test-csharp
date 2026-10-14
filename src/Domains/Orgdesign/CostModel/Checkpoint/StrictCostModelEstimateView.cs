namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict cost model estimate.</summary>
public sealed record StrictCostModelEstimateView(string CodeText, string TitleText, string Version, string LimitText, string Rate, string Phone, string PinnedText, string StatusLabel)
{
    public static StrictCostModelEstimateView ToStrictCostModelEstimateView(StrictCostModelEstimate item)
    {
        return new StrictCostModelEstimateView(
            item.Code,
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 120),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictCostModelEstimateView> ToStrictCostModelEstimateViews(List<StrictCostModelEstimate> items)
    {
        var views = new List<StrictCostModelEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToStrictCostModelEstimateView(item));
        }
        return views;
    }

    public static double SumStrictCostModelEstimateView(StrictCostModelEstimateView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedLimit + parsedRate;
    }
}
