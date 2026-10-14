namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat group.</summary>
public sealed record LicenseSeatGroupView(string Code, string LabelText, string CreatedAtText, string TenureMonths, string Discount, string Sequence, string Location, string Region, string EndsAt, string AttemptsText, string Badge, string StatusLabel)
{
    public static LicenseSeatGroupView ToLicenseSeatGroupView(LicenseSeatGroup item)
    {
        return new LicenseSeatGroupView(
            item.Code,
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.Region, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatGroupView> ToLicenseSeatGroupViews(List<LicenseSeatGroup> items)
    {
        var views = new List<LicenseSeatGroupView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatGroupView(item));
        }
        return views;
    }

    public static double SumLicenseSeatGroupView(LicenseSeatGroupView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedTenureMonths + parsedDiscount;
    }
}
