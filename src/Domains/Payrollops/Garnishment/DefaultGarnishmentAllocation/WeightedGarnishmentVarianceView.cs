namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted garnishment variance.</summary>
public sealed record WeightedGarnishmentVarianceView(string Code, string Title, string SegmentText, string CreatedAt, string AttemptsText, string Name, string GraceDaysText, string SequenceText, string Fte, string RateText, string StatusLabel)
{
    public static WeightedGarnishmentVarianceView ToWeightedGarnishmentVarianceView(WeightedGarnishmentVariance item)
    {
        return new WeightedGarnishmentVarianceView(
            item.Code,
            item.Title,
            Text.Truncate(item.Segment, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedGarnishmentVarianceView> ToWeightedGarnishmentVarianceViews(List<WeightedGarnishmentVariance> items)
    {
        var views = new List<WeightedGarnishmentVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedGarnishmentVarianceView(item));
        }
        return views;
    }

    public static double SumWeightedGarnishmentVarianceView(WeightedGarnishmentVarianceView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedAttempts + (double)parsedGraceDays;
    }
}
