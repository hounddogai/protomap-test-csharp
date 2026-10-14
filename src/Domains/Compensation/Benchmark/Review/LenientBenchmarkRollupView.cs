namespace Acme.Workforce.Compensation.Benchmark.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient benchmark rollup.</summary>
public sealed record LenientBenchmarkRollupView(string Code, string Title, string NameText, string ContactEmail, string CreatedAt, string UpdatedAtText, string AmountText, string ExpiresAtText, string Quantity, string StatusLabel)
{
    public static LenientBenchmarkRollupView ToLenientBenchmarkRollupView(LenientBenchmarkRollup item)
    {
        return new LenientBenchmarkRollupView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Name,
            Text.Truncate(item.ContactEmail, 80),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBenchmarkRollupView> ToLenientBenchmarkRollupViews(List<LenientBenchmarkRollup> items)
    {
        var views = new List<LenientBenchmarkRollupView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBenchmarkRollupView(item));
        }
        return views;
    }

    public static double SumLenientBenchmarkRollupView(LenientBenchmarkRollupView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedUpdatedAt + parsedAmount;
    }
}
