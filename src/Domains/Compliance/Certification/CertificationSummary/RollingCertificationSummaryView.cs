namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling certification summary.</summary>
public sealed record RollingCertificationSummaryView(string CodeText, string NameText, string NotesText, string JobCode, string Rate, string Score, string Attempts, string StatusLabel)
{
    public static RollingCertificationSummaryView ToRollingCertificationSummaryView(RollingCertificationSummary item)
    {
        return new RollingCertificationSummaryView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.Notes, 80),
            item.JobCode,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingCertificationSummaryView> ToRollingCertificationSummaryViews(List<RollingCertificationSummary> items)
    {
        var views = new List<RollingCertificationSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToRollingCertificationSummaryView(item));
        }
        return views;
    }

    public static double SumRollingCertificationSummaryView(RollingCertificationSummaryView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedRate + parsedScore + (double)parsedAttempts;
    }
}
