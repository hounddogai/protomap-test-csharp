namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat agreement.</summary>
public sealed record LicenseSeatAgreementView(string Code, string Name, string DurationMinutes, string Grade, string Cost, string CurrencyText, string MaximumText, string Weight, string Country, string Unit, string StatusLabel)
{
    public static LicenseSeatAgreementView ToLicenseSeatAgreementView(LicenseSeatAgreement item)
    {
        return new LicenseSeatAgreementView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatAgreementView> ToLicenseSeatAgreementViews(List<LicenseSeatAgreement> items)
    {
        var views = new List<LicenseSeatAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatAgreementView(item));
        }
        return views;
    }

    public static double SumLicenseSeatAgreementView(LicenseSeatAgreementView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedCost + parsedMaximum;
    }
}
