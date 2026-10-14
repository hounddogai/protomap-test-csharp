namespace Acme.Workforce.Workplace.Booking.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped booking agreement.</summary>
public sealed record CappedBookingAgreementView(string CodeText, string Name, string Description, string EmployeeId, string ExternalRef, string LeaveDays, string Checksum, string Sequence, string Cost, string AttemptsText, string StatusLabel)
{
    public static CappedBookingAgreementView ToCappedBookingAgreementView(CappedBookingAgreement item)
    {
        return new CappedBookingAgreementView(
            item.Code,
            item.Name,
            Text.Truncate(item.Description, 64),
            item.EmployeeId,
            item.ExternalRef,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedBookingAgreementView> ToCappedBookingAgreementViews(List<CappedBookingAgreement> items)
    {
        var views = new List<CappedBookingAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToCappedBookingAgreementView(item));
        }
        return views;
    }

    public static double SumCappedBookingAgreementView(CappedBookingAgreementView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedSequence + parsedCost;
    }
}
