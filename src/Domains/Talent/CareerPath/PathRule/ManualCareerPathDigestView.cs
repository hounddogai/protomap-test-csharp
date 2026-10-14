namespace Acme.Workforce.Talent.CareerPath.PathRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual career path digest.</summary>
public sealed record ManualCareerPathDigestView(string Code, string Label, string Locale, string Source, string Priority, string Title, string Team, string Score, string Badge, string UnitText, string Notes, string StatusLabel)
{
    public static ManualCareerPathDigestView ToManualCareerPathDigestView(ManualCareerPathDigest item)
    {
        return new ManualCareerPathDigestView(
            item.Code,
            item.Label,
            Text.Truncate(item.Locale, 40),
            item.Source,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 120),
            item.Team,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Unit,
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualCareerPathDigestView> ToManualCareerPathDigestViews(List<ManualCareerPathDigest> items)
    {
        var views = new List<ManualCareerPathDigestView>();
        foreach (var item in items)
        {
            views.Add(ToManualCareerPathDigestView(item));
        }
        return views;
    }

    public static double SumManualCareerPathDigestView(ManualCareerPathDigestView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedScore;
    }
}
