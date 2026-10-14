namespace Acme.Workforce.Talent.Nomination.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched nomination forecast.</summary>
public sealed record BatchedNominationForecastView(string Code, string Title, string CapacityText, string OwnerIdText, string Region, string LimitText, string Attempts, string Pinned, string Currency, string LeaveDays, string Fte, string StatusLabel)
{
    public static BatchedNominationForecastView ToBatchedNominationForecastView(BatchedNominationForecast item)
    {
        return new BatchedNominationForecastView(
            item.Code,
            item.Title,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Region,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Currency, 120),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedNominationForecastView> ToBatchedNominationForecastViews(List<BatchedNominationForecast> items)
    {
        var views = new List<BatchedNominationForecastView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedNominationForecastView(item));
        }
        return views;
    }

    public static double SumBatchedNominationForecastView(BatchedNominationForecastView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedLimit + (double)parsedAttempts;
    }
}
