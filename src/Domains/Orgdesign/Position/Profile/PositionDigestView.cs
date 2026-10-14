namespace Acme.Workforce.Orgdesign.Position.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position digest.</summary>
public sealed record PositionDigestView(string Code, string Name, string EmployeeId, string Archived, string Locale, string Visible, string Approved, string Score, string StatusLabel)
{
    public static PositionDigestView ToPositionDigestView(PositionDigest item)
    {
        return new PositionDigestView(
            item.Code,
            item.Name,
            Text.Truncate(item.EmployeeId, 40),
            $"{item.Archived}",
            item.Locale,
            $"{item.Visible}",
            $"{item.Approved}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionDigestView> ToPositionDigestViews(List<PositionDigest> items)
    {
        var views = new List<PositionDigestView>();
        foreach (var item in items)
        {
            views.Add(ToPositionDigestView(item));
        }
        return views;
    }

    public static double SumPositionDigestView(PositionDigestView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
