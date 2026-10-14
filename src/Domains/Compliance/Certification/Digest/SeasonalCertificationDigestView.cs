namespace Acme.Workforce.Compliance.Certification.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal certification digest.</summary>
public sealed record SeasonalCertificationDigestView(string Code, string Label, string Team, string DurationMinutes, string PriorityText, string StatusLabel)
{
    public static SeasonalCertificationDigestView ToSeasonalCertificationDigestView(SeasonalCertificationDigest item)
    {
        return new SeasonalCertificationDigestView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Team,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalCertificationDigestView> ToSeasonalCertificationDigestViews(List<SeasonalCertificationDigest> items)
    {
        var views = new List<SeasonalCertificationDigestView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalCertificationDigestView(item));
        }
        return views;
    }

    public static double SumSeasonalCertificationDigestView(SeasonalCertificationDigestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedPriority;
    }
}
