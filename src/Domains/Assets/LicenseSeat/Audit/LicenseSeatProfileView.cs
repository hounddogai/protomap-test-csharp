namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat profile.</summary>
public sealed record LicenseSeatProfileView(string Code, string TitleText, string Category, string RegionText, string Cost, string Minimum, string LocaleText, string Label, string Ratio, string StatusLabel)
{
    public static LicenseSeatProfileView ToLicenseSeatProfileView(LicenseSeatProfile item)
    {
        return new LicenseSeatProfileView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Category,
            Text.Truncate(item.Region, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatProfileView> ToLicenseSeatProfileViews(List<LicenseSeatProfile> items)
    {
        var views = new List<LicenseSeatProfileView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatProfileView(item));
        }
        return views;
    }

    public static double SumLicenseSeatProfileView(LicenseSeatProfileView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedCost + parsedMinimum + parsedRatio;
    }
}
