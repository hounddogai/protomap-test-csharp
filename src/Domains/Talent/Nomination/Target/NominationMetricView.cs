namespace Acme.Workforce.Talent.Nomination.Target;

using System.Globalization;

/// <summary>How screens and exports show a nomination metric.</summary>
public sealed record NominationMetricView(string Code, string Name, string BatchSize, string Approved, string StartsAt, string Cost, string ExpiresAt, string Sequence, string TenureMonths, string ArchivedText, string StatusLabel)
{
    public static NominationMetricView ToNominationMetricView(NominationMetric item)
    {
        return new NominationMetricView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationMetricView> ToNominationMetricViews(List<NominationMetric> items)
    {
        var views = new List<NominationMetricView>();
        foreach (var item in items)
        {
            views.Add(ToNominationMetricView(item));
        }
        return views;
    }

    public static double SumNominationMetricView(NominationMetricView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedStartsAt + parsedCost;
    }
}
