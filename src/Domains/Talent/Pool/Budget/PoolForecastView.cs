namespace Acme.Workforce.Talent.Pool.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool forecast.</summary>
public sealed record PoolForecastView(string Code, string NameText, string Category, string BadgeText, string Reviewer, string Retries, string DiscountText, string Weight, string StatusLabel)
{
    public static PoolForecastView ToPoolForecastView(PoolForecast item)
    {
        return new PoolForecastView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Category, 120),
            item.Badge,
            item.Reviewer,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolForecastView> ToPoolForecastViews(List<PoolForecast> items)
    {
        var views = new List<PoolForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPoolForecastView(item));
        }
        return views;
    }

    public static double SumPoolForecastView(PoolForecastView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedDiscount + parsedWeight;
    }
}
