namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a primary benchmark claim.</summary>
public sealed record PrimaryBenchmarkClaimView(string Code, string Title, string Location, string OwnerId, string LeaveDays, string StatusLabel)
{
    public static PrimaryBenchmarkClaimView ToPrimaryBenchmarkClaimView(PrimaryBenchmarkClaim item)
    {
        return new PrimaryBenchmarkClaimView(
            item.Code,
            item.Title,
            item.Location,
            item.OwnerId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryBenchmarkClaimView> ToPrimaryBenchmarkClaimViews(List<PrimaryBenchmarkClaim> items)
    {
        var views = new List<PrimaryBenchmarkClaimView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryBenchmarkClaimView(item));
        }
        return views;
    }

    public static double SumPrimaryBenchmarkClaimView(PrimaryBenchmarkClaimView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
