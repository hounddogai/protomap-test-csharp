namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using System.Globalization;

/// <summary>How screens and exports show a rolling garnishment variance.</summary>
public sealed record RollingGarnishmentVarianceView(string CodeText, string Title, string Hours, string Description, string CategoryText, string Source, string StatusLabel)
{
    public static RollingGarnishmentVarianceView ToRollingGarnishmentVarianceView(RollingGarnishmentVariance item)
    {
        return new RollingGarnishmentVarianceView(
            item.Code,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Category,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingGarnishmentVarianceView> ToRollingGarnishmentVarianceViews(List<RollingGarnishmentVariance> items)
    {
        var views = new List<RollingGarnishmentVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRollingGarnishmentVarianceView(item));
        }
        return views;
    }

    public static double SumRollingGarnishmentVarianceView(RollingGarnishmentVarianceView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedHours;
    }
}
