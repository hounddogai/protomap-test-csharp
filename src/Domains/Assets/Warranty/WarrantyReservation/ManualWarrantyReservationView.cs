namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual warranty reservation.</summary>
public sealed record ManualWarrantyReservationView(string CodeText, string LabelText, string Maximum, string Reviewer, string Version, string ExpiresAt, string PhoneText, string CreatedAtText, string ManagerIdText, string Weight, string LocaleText, string StatusLabel)
{
    public static ManualWarrantyReservationView ToManualWarrantyReservationView(ManualWarrantyReservation item)
    {
        return new ManualWarrantyReservationView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualWarrantyReservationView> ToManualWarrantyReservationViews(List<ManualWarrantyReservation> items)
    {
        var views = new List<ManualWarrantyReservationView>();
        foreach (var item in items)
        {
            views.Add(ToManualWarrantyReservationView(item));
        }
        return views;
    }

    public static double SumManualWarrantyReservationView(ManualWarrantyReservationView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedVersion + (double)parsedExpiresAt;
    }
}
