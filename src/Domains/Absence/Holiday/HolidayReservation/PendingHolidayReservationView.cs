namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending holiday reservation.</summary>
public sealed record PendingHolidayReservationView(string Code, string Title, string ApprovedText, string EmployeeId, string DepartmentText, string Rate, string CreatedAt, string NotesText, string ExpiresAt, string StatusLabel)
{
    public static PendingHolidayReservationView ToPendingHolidayReservationView(PendingHolidayReservation item)
    {
        return new PendingHolidayReservationView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            item.EmployeeId,
            item.Department,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 64),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingHolidayReservationView> ToPendingHolidayReservationViews(List<PendingHolidayReservation> items)
    {
        var views = new List<PendingHolidayReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPendingHolidayReservationView(item));
        }
        return views;
    }

    public static double SumPendingHolidayReservationView(PendingHolidayReservationView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedCreatedAt + (double)parsedExpiresAt;
    }
}
