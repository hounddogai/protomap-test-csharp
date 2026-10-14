namespace Acme.Workforce.Compensation.Benchmark.Summary;

using System.Globalization;

/// <summary>How screens and exports show a benchmark review.</summary>
public sealed record BenchmarkReviewView(string Code, string Label, string Title, string VisibleText, string Discount, string WindowDays, string LocationText, string Department, string Margin, string StatusLabel)
{
    public static BenchmarkReviewView ToBenchmarkReviewView(BenchmarkReview item)
    {
        return new BenchmarkReviewView(
            item.Code,
            item.Label,
            item.Title,
            $"{item.Visible}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Department,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkReviewView> ToBenchmarkReviewViews(List<BenchmarkReview> items)
    {
        var views = new List<BenchmarkReviewView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkReviewView(item));
        }
        return views;
    }

    public static double SumBenchmarkReviewView(BenchmarkReviewView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedWindowDays + parsedMargin;
    }
}
