namespace Acme.Workforce.Absence.LeaveRequest.Window;

using System.Globalization;

/// <summary>How screens and exports show a lenient leave request reservation.</summary>
public sealed record LenientLeaveRequestReservationView(string Code, string Title, string Active, string TenureMonths, string Budget, string CostCenter, string StatusLabel)
{
    public static LenientLeaveRequestReservationView ToLenientLeaveRequestReservationView(LenientLeaveRequestReservation item)
    {
        return new LenientLeaveRequestReservationView(
            item.Code,
            item.Title,
            $"{item.Active}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientLeaveRequestReservationView> ToLenientLeaveRequestReservationViews(List<LenientLeaveRequestReservation> items)
    {
        var views = new List<LenientLeaveRequestReservationView>();
        foreach (var item in items)
        {
            views.Add(ToLenientLeaveRequestReservationView(item));
        }
        return views;
    }

    public static double SumLenientLeaveRequestReservationView(LenientLeaveRequestReservationView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedBudget;
    }
}
