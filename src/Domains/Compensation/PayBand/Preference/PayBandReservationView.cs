namespace Acme.Workforce.Compensation.PayBand.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band reservation.</summary>
public sealed record PayBandReservationView(string Code, string Name, string Channel, string Budget, string JobCode, string DurationMinutes, string TenureMonths, string SalaryBandText, string Timezone, string Retries, string ManagerIdText, string StatusLabel)
{
    public static PayBandReservationView ToPayBandReservationView(PayBandReservation item)
    {
        return new PayBandReservationView(
            item.Code,
            item.Name,
            item.Channel,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 120),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            Text.Truncate(item.Timezone, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandReservationView> ToPayBandReservationViews(List<PayBandReservation> items)
    {
        var views = new List<PayBandReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandReservationView(item));
        }
        return views;
    }

    public static double SumPayBandReservationView(PayBandReservationView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedDurationMinutes + (double)parsedTenureMonths;
    }
}
