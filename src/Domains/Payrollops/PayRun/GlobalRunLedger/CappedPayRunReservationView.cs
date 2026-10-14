namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped pay run reservation.</summary>
public sealed record CappedPayRunReservationView(string Code, string Title, string Maximum, string BatchSizeText, string Summary, string Quantity, string Region, string FloorValue, string GraceDays, string StatusLabel)
{
    public static CappedPayRunReservationView ToCappedPayRunReservationView(CappedPayRunReservation item)
    {
        return new CappedPayRunReservationView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPayRunReservationView> ToCappedPayRunReservationViews(List<CappedPayRunReservation> items)
    {
        var views = new List<CappedPayRunReservationView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPayRunReservationView(item));
        }
        return views;
    }

    public static double SumCappedPayRunReservationView(CappedPayRunReservationView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedBatchSize + (double)parsedQuantity;
    }
}
