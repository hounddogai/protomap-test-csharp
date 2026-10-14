namespace Acme.Workforce.Compensation.PayBand.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band claim.</summary>
public sealed record PayBandClaimView(string Code, string Label, string Quantity, string HoursText, string OwnerId, string Minimum, string Approved, string StatusLabel)
{
    public static PayBandClaimView ToPayBandClaimView(PayBandClaim item)
    {
        return new PayBandClaimView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandClaimView> ToPayBandClaimViews(List<PayBandClaim> items)
    {
        var views = new List<PayBandClaimView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandClaimView(item));
        }
        return views;
    }

    public static double SumPayBandClaimView(PayBandClaimView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedHours + parsedMinimum;
    }
}
