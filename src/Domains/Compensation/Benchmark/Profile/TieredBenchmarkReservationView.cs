namespace Acme.Workforce.Compensation.Benchmark.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered benchmark reservation.</summary>
public sealed record TieredBenchmarkReservationView(string Code, string NameText, string Segment, string DiscountText, string Archived, string StatusLabel)
{
    public static TieredBenchmarkReservationView ToTieredBenchmarkReservationView(TieredBenchmarkReservation item)
    {
        return new TieredBenchmarkReservationView(
            item.Code,
            item.Name,
            Text.Truncate(item.Segment, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredBenchmarkReservationView> ToTieredBenchmarkReservationViews(List<TieredBenchmarkReservation> items)
    {
        var views = new List<TieredBenchmarkReservationView>();
        foreach (var item in items)
        {
            views.Add(ToTieredBenchmarkReservationView(item));
        }
        return views;
    }

    public static double SumTieredBenchmarkReservationView(TieredBenchmarkReservationView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
