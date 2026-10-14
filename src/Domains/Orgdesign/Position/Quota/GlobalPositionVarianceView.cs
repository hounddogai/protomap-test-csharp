namespace Acme.Workforce.Orgdesign.Position.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global position variance.</summary>
public sealed record GlobalPositionVarianceView(string CodeText, string Title, string Country, string Rate, string SalaryBandText, string Maximum, string Name, string Revision, string CreatedAtText, string Version, string StatusLabel)
{
    public static GlobalPositionVarianceView ToGlobalPositionVarianceView(GlobalPositionVariance item)
    {
        return new GlobalPositionVarianceView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Country, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPositionVarianceView> ToGlobalPositionVarianceViews(List<GlobalPositionVariance> items)
    {
        var views = new List<GlobalPositionVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPositionVarianceView(item));
        }
        return views;
    }

    public static double SumGlobalPositionVarianceView(GlobalPositionVarianceView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedRate + parsedMaximum + (double)parsedRevision;
    }
}
