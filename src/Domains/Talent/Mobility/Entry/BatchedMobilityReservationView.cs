namespace Acme.Workforce.Talent.Mobility.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched mobility reservation.</summary>
public sealed record BatchedMobilityReservationView(string Code, string Title, string WindowDaysText, string Maximum, string Locale, string BatchSize, string StatusLabel)
{
    public static BatchedMobilityReservationView ToBatchedMobilityReservationView(BatchedMobilityReservation item)
    {
        return new BatchedMobilityReservationView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedMobilityReservationView> ToBatchedMobilityReservationViews(List<BatchedMobilityReservation> items)
    {
        var views = new List<BatchedMobilityReservationView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedMobilityReservationView(item));
        }
        return views;
    }

    public static double SumBatchedMobilityReservationView(BatchedMobilityReservationView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMaximum + (double)parsedBatchSize;
    }
}
