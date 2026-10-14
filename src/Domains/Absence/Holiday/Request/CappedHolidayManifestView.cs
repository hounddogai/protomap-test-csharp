namespace Acme.Workforce.Absence.Holiday.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped holiday manifest.</summary>
public sealed record CappedHolidayManifestView(string Code, string LabelText, string Rate, string PinnedText, string Segment, string StatusLabel)
{
    public static CappedHolidayManifestView ToCappedHolidayManifestView(CappedHolidayManifest item)
    {
        return new CappedHolidayManifestView(
            item.Code,
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Segment, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedHolidayManifestView> ToCappedHolidayManifestViews(List<CappedHolidayManifest> items)
    {
        var views = new List<CappedHolidayManifestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedHolidayManifestView(item));
        }
        return views;
    }

    public static double SumCappedHolidayManifestView(CappedHolidayManifestView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
