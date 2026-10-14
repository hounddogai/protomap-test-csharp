namespace Acme.Workforce.Workplace.Desk.Budget;

using System.Globalization;

/// <summary>How screens and exports show a desk variance.</summary>
public sealed record DeskVarianceView(string Code, string Name, string Ratio, string Margin, string Version, string HoursText, string StatusLabel)
{
    public static DeskVarianceView ToDeskVarianceView(DeskVariance item)
    {
        return new DeskVarianceView(
            item.Code,
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskVarianceView> ToDeskVarianceViews(List<DeskVariance> items)
    {
        var views = new List<DeskVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToDeskVarianceView(item));
        }
        return views;
    }

    public static double SumDeskVarianceView(DeskVarianceView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedRatio + parsedMargin + (double)parsedVersion;
    }
}
