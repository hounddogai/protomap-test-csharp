namespace Acme.Workforce.Workplace.Occupancy.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy profile.</summary>
public sealed record OccupancyProfileView(string Code, string Title, string TenureMonths, string DescriptionText, string Revision, string FteText, string StatusLabel)
{
    public static OccupancyProfileView ToOccupancyProfileView(OccupancyProfile item)
    {
        return new OccupancyProfileView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyProfileView> ToOccupancyProfileViews(List<OccupancyProfile> items)
    {
        var views = new List<OccupancyProfileView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyProfileView(item));
        }
        return views;
    }

    public static double SumOccupancyProfileView(OccupancyProfileView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedRevision + parsedFte;
    }
}
