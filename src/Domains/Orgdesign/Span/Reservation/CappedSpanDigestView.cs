namespace Acme.Workforce.Orgdesign.Span.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped span digest.</summary>
public sealed record CappedSpanDigestView(string CodeText, string TitleText, string FloorValue, string Limit, string MinimumText, string LeaveDays, string WindowDays, string Department, string Name, string Rating, string StatusLabel)
{
    public static CappedSpanDigestView ToCappedSpanDigestView(CappedSpanDigest item)
    {
        return new CappedSpanDigestView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 120),
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedSpanDigestView> ToCappedSpanDigestViews(List<CappedSpanDigest> items)
    {
        var views = new List<CappedSpanDigestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedSpanDigestView(item));
        }
        return views;
    }

    public static double SumCappedSpanDigestView(CappedSpanDigestView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedLimit + parsedMinimum;
    }
}
