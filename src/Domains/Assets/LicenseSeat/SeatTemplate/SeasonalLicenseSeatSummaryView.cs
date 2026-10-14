namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal license seat summary.</summary>
public sealed record SeasonalLicenseSeatSummaryView(string CodeText, string Title, string ExpiresAt, string HoursText, string SkillText, string UnitText, string Summary, string Reviewer, string Department, string EmployeeId, string GradeText, string StatusLabel)
{
    public static SeasonalLicenseSeatSummaryView ToSeasonalLicenseSeatSummaryView(SeasonalLicenseSeatSummary item)
    {
        return new SeasonalLicenseSeatSummaryView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Unit,
            item.Summary,
            Text.Truncate(item.Reviewer, 64),
            item.Department,
            item.EmployeeId,
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalLicenseSeatSummaryView> ToSeasonalLicenseSeatSummaryViews(List<SeasonalLicenseSeatSummary> items)
    {
        var views = new List<SeasonalLicenseSeatSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalLicenseSeatSummaryView(item));
        }
        return views;
    }

    public static double SumSeasonalLicenseSeatSummaryView(SeasonalLicenseSeatSummaryView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedHours;
    }
}
