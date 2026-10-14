namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using System.Globalization;

/// <summary>How screens and exports show a capped audit reservation.</summary>
public sealed record CappedAuditReservationView(string Code, string Name, string Department, string BatchSize, string ChannelText, string Capacity, string FteText, string Attempts, string Weight, string Title, string Discount, string StatusLabel)
{
    public static CappedAuditReservationView ToCappedAuditReservationView(CappedAuditReservation item)
    {
        return new CappedAuditReservationView(
            item.Code,
            item.Name,
            item.Department,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAuditReservationView> ToCappedAuditReservationViews(List<CappedAuditReservation> items)
    {
        var views = new List<CappedAuditReservationView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAuditReservationView(item));
        }
        return views;
    }

    public static double SumCappedAuditReservationView(CappedAuditReservationView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedCapacity + parsedFte;
    }
}
