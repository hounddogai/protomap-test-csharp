namespace Acme.Workforce.Workplace.Desk.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk digest.</summary>
public sealed record DeskDigestView(string CodeText, string Label, string Visible, string RevisionText, string Cost, string Region, string Country, string DescriptionText, string Ceiling, string Version, string TenureMonthsText, string StatusLabel)
{
    public static DeskDigestView ToDeskDigestView(DeskDigest item)
    {
        return new DeskDigestView(
            Text.Truncate(item.Code, 64),
            item.Label,
            $"{item.Visible}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            Text.Truncate(item.Country, 80),
            Text.Truncate(item.Description, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskDigestView> ToDeskDigestViews(List<DeskDigest> items)
    {
        var views = new List<DeskDigestView>();
        foreach (var item in items)
        {
            views.Add(ToDeskDigestView(item));
        }
        return views;
    }

    public static double SumDeskDigestView(DeskDigestView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedCost + parsedCeiling;
    }
}
