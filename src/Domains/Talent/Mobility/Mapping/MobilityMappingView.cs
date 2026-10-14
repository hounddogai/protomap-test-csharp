namespace Acme.Workforce.Talent.Mobility.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility mapping.</summary>
public sealed record MobilityMappingView(string Code, string Title, string LimitText, string Active, string LocationText, string EmployeeId, string Revision, string Rating, string Amount, string StatusLabel)
{
    public static MobilityMappingView ToMobilityMappingView(MobilityMapping item)
    {
        return new MobilityMappingView(
            item.Code,
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Location,
            Text.Truncate(item.EmployeeId, 40),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityMappingView> ToMobilityMappingViews(List<MobilityMapping> items)
    {
        var views = new List<MobilityMappingView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityMappingView(item));
        }
        return views;
    }

    public static double SumMobilityMappingView(MobilityMappingView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedRevision + parsedRating;
    }
}
