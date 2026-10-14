namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default license seat profile.</summary>
public sealed record DefaultLicenseSeatProfileView(string Code, string Title, string Source, string LocationText, string Threshold, string Rate, string CapacityText, string Team, string ContactEmailText, string StatusLabel)
{
    public static DefaultLicenseSeatProfileView ToDefaultLicenseSeatProfileView(DefaultLicenseSeatProfile item)
    {
        return new DefaultLicenseSeatProfileView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Source,
            item.Location,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultLicenseSeatProfileView> ToDefaultLicenseSeatProfileViews(List<DefaultLicenseSeatProfile> items)
    {
        var views = new List<DefaultLicenseSeatProfileView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultLicenseSeatProfileView(item));
        }
        return views;
    }

    public static double SumDefaultLicenseSeatProfileView(DefaultLicenseSeatProfileView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedRate + (double)parsedCapacity;
    }
}
