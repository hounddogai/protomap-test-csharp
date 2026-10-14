namespace Acme.Workforce.Talent.Pool.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool rollup.</summary>
public sealed record PoolRollupView(string Code, string Title, string Country, string Unit, string EndsAtText, string Retries, string EmployeeId, string TenureMonths, string Team, string CostCenter, string StatusLabel)
{
    public static PoolRollupView ToPoolRollupView(PoolRollup item)
    {
        return new PoolRollupView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            item.Country,
            item.Unit,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolRollupView> ToPoolRollupViews(List<PoolRollup> items)
    {
        var views = new List<PoolRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPoolRollupView(item));
        }
        return views;
    }

    public static double SumPoolRollupView(PoolRollupView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRetries + (double)parsedTenureMonths;
    }
}
