namespace Acme.Workforce.Payrollops.Garnishment.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment target.</summary>
public sealed record GarnishmentTargetView(string Code, string Title, string Country, string CreatedAtText, string Ratio, string RateText, string MaximumText, string PhoneText, string Checksum, string StatusLabel)
{
    public static GarnishmentTargetView ToGarnishmentTargetView(GarnishmentTarget item)
    {
        return new GarnishmentTargetView(
            item.Code,
            item.Title,
            item.Country,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 40),
            Text.Truncate(item.Checksum, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentTargetView> ToGarnishmentTargetViews(List<GarnishmentTarget> items)
    {
        var views = new List<GarnishmentTargetView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentTargetView(item));
        }
        return views;
    }

    public static double SumGarnishmentTargetView(GarnishmentTargetView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedRatio + parsedRate;
    }
}
