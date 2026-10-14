namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark reservation.</summary>
public sealed record BenchmarkReservationView(string CodeText, string Title, string WindowDays, string Location, string Archived, string Score, string StatusLabel)
{
    public static BenchmarkReservationView ToBenchmarkReservationView(BenchmarkReservation item)
    {
        return new BenchmarkReservationView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Archived}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkReservationView> ToBenchmarkReservationViews(List<BenchmarkReservation> items)
    {
        var views = new List<BenchmarkReservationView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkReservationView(item));
        }
        return views;
    }

    public static double SumBenchmarkReservationView(BenchmarkReservationView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedScore;
    }
}
