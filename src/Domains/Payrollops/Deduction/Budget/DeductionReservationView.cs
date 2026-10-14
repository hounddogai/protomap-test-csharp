namespace Acme.Workforce.Payrollops.Deduction.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction reservation.</summary>
public sealed record DeductionReservationView(string Code, string Name, string Channel, string Quantity, string RateText, string Badge, string Limit, string HoursText, string StatusLabel)
{
    public static DeductionReservationView ToDeductionReservationView(DeductionReservation item)
    {
        return new DeductionReservationView(
            item.Code,
            item.Name,
            Text.Truncate(item.Channel, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionReservationView> ToDeductionReservationViews(List<DeductionReservation> items)
    {
        var views = new List<DeductionReservationView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionReservationView(item));
        }
        return views;
    }

    public static double SumDeductionReservationView(DeductionReservationView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedRate + (double)parsedLimit;
    }
}
