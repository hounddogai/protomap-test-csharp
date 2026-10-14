namespace Acme.Workforce.Talent.Pool.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool estimate.</summary>
public sealed record PoolEstimateView(string Code, string Title, string Limit, string JobCodeText, string Region, string StatusLabel)
{
    public static PoolEstimateView ToPoolEstimateView(PoolEstimate item)
    {
        return new PoolEstimateView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            Text.Truncate(item.Region, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolEstimateView> ToPoolEstimateViews(List<PoolEstimate> items)
    {
        var views = new List<PoolEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToPoolEstimateView(item));
        }
        return views;
    }

    public static double SumPoolEstimateView(PoolEstimateView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
