namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary license seat window.</summary>
public sealed record PrimaryLicenseSeatWindowView(string CodeText, string Label, string RetriesText, string Location, string EndsAt, string Rating, string CostCenter, string EmployeeIdText, string LeaveDays, string AmountText, string Currency, string StatusLabel)
{
    public static PrimaryLicenseSeatWindowView ToPrimaryLicenseSeatWindowView(PrimaryLicenseSeatWindow item)
    {
        return new PrimaryLicenseSeatWindowView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.EmployeeId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryLicenseSeatWindowView> ToPrimaryLicenseSeatWindowViews(List<PrimaryLicenseSeatWindow> items)
    {
        var views = new List<PrimaryLicenseSeatWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryLicenseSeatWindowView(item));
        }
        return views;
    }

    public static double SumPrimaryLicenseSeatWindowView(PrimaryLicenseSeatWindowView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedEndsAt + parsedRating;
    }
}
