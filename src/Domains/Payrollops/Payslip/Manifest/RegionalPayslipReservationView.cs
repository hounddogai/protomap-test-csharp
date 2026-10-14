namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional payslip reservation.</summary>
public sealed record RegionalPayslipReservationView(string Code, string LabelText, string Limit, string SalaryBand, string GraceDaysText, string Priority, string CostCenter, string FloorValue, string Locale, string Threshold, string RevisionText, string StatusLabel)
{
    public static RegionalPayslipReservationView ToRegionalPayslipReservationView(RegionalPayslipReservation item)
    {
        return new RegionalPayslipReservationView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPayslipReservationView> ToRegionalPayslipReservationViews(List<RegionalPayslipReservation> items)
    {
        var views = new List<RegionalPayslipReservationView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPayslipReservationView(item));
        }
        return views;
    }

    public static double SumRegionalPayslipReservationView(RegionalPayslipReservationView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedGraceDays + (double)parsedPriority;
    }
}
