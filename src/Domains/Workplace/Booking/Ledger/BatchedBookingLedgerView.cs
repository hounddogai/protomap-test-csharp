namespace Acme.Workforce.Workplace.Booking.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched booking ledger.</summary>
public sealed record BatchedBookingLedgerView(string Code, string Label, string Segment, string Location, string Pinned, string Priority, string Name, string JobCodeText, string NotesText, string Limit, string Minimum, string StatusLabel)
{
    public static BatchedBookingLedgerView ToBatchedBookingLedgerView(BatchedBookingLedger item)
    {
        return new BatchedBookingLedgerView(
            item.Code,
            item.Label,
            item.Segment,
            item.Location,
            $"{item.Pinned}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.JobCode, 64),
            Text.Truncate(item.Notes, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedBookingLedgerView> ToBatchedBookingLedgerViews(List<BatchedBookingLedger> items)
    {
        var views = new List<BatchedBookingLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedBookingLedgerView(item));
        }
        return views;
    }

    public static double SumBatchedBookingLedgerView(BatchedBookingLedgerView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedLimit + parsedMinimum;
    }
}
