namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy booking audit.</summary>
public sealed record LegacyBookingAuditView(string Code, string TitleText, string OwnerId, string Sequence, string Visible, string Name, string QuantityText, string CreatedAtText, string ExternalRefText, string BatchSize, string StatusLabel)
{
    public static LegacyBookingAuditView ToLegacyBookingAuditView(LegacyBookingAudit item)
    {
        return new LegacyBookingAuditView(
            item.Code,
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.OwnerId, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyBookingAuditView> ToLegacyBookingAuditViews(List<LegacyBookingAudit> items)
    {
        var views = new List<LegacyBookingAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyBookingAuditView(item));
        }
        return views;
    }

    public static double SumLegacyBookingAuditView(LegacyBookingAuditView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedQuantity + (double)parsedCreatedAt;
    }
}
