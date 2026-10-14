namespace Acme.Workforce.Compensation.Benchmark.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark notice.</summary>
public sealed record BenchmarkNoticeView(string CodeText, string TitleText, string Discount, string ChannelText, string HoursText, string Threshold, string CreatedAtText, string Name, string ManagerIdText, string StatusLabel)
{
    public static BenchmarkNoticeView ToBenchmarkNoticeView(BenchmarkNotice item)
    {
        return new BenchmarkNoticeView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkNoticeView> ToBenchmarkNoticeViews(List<BenchmarkNotice> items)
    {
        var views = new List<BenchmarkNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkNoticeView(item));
        }
        return views;
    }

    public static double SumBenchmarkNoticeView(BenchmarkNoticeView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedHours + parsedThreshold;
    }
}
