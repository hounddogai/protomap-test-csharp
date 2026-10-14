namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary license seat cycle.</summary>
public sealed record PrimaryLicenseSeatCycleView(string Code, string NameText, string Summary, string ApprovedText, string Department, string StartsAtText, string ContactEmail, string Weight, string Channel, string Minimum, string StatusLabel)
{
    public static PrimaryLicenseSeatCycleView ToPrimaryLicenseSeatCycleView(PrimaryLicenseSeatCycle item)
    {
        return new PrimaryLicenseSeatCycleView(
            item.Code,
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Summary, 80),
            $"{item.Approved}",
            item.Department,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryLicenseSeatCycleView> ToPrimaryLicenseSeatCycleViews(List<PrimaryLicenseSeatCycle> items)
    {
        var views = new List<PrimaryLicenseSeatCycleView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryLicenseSeatCycleView(item));
        }
        return views;
    }

    public static double SumPrimaryLicenseSeatCycleView(PrimaryLicenseSeatCycleView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedWeight + parsedMinimum;
    }
}
