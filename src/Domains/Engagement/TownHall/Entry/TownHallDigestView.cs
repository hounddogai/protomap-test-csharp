namespace Acme.Workforce.Engagement.TownHall.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall digest.</summary>
public sealed record TownHallDigestView(string Code, string Label, string ManagerIdText, string GradeText, string Score, string Version, string Margin, string CostCenter, string EndsAtText, string StatusLabel)
{
    public static TownHallDigestView ToTownHallDigestView(TownHallDigest item)
    {
        return new TownHallDigestView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.ManagerId,
            item.Grade,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallDigestView> ToTownHallDigestViews(List<TownHallDigest> items)
    {
        var views = new List<TownHallDigestView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallDigestView(item));
        }
        return views;
    }

    public static double SumTownHallDigestView(TownHallDigestView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedVersion + parsedMargin;
    }
}
